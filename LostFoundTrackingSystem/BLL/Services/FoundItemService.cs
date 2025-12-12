using BLL.DTOs.FoundItemDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;

namespace BLL.Services
{
    public class FoundItemService : IFoundItemService
    {
        private readonly IFoundItemRepository _repo;
        private readonly IImageRepository _imageRepo;
        private readonly IImageService _imageService;

        public FoundItemService(IFoundItemRepository repo, IImageRepository imageRepo, IImageService imageService)
        {
            _repo = repo;
            _imageRepo = imageRepo;
            _imageService = imageService;
        }

        public async Task<List<FoundItemDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return MapToDtoList(items);
        }

        public async Task<FoundItemDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;

            return new FoundItemDto
            {
                FoundItemId = item.FoundItemId,
                Title = item.Title,
                Description = item.Description,
                FoundDate = item.FoundDate,
                FoundLocation = item.FoundLocation,
                Status = item.Status,
                CampusId = item.CampusId,
                CampusName = item.Campus?.CampusName,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.CategoryName,
                CreatedBy = item.CreatedBy,
                StoredBy = item.StoredBy,
                ImageUrls = item.Images.Select(i => i.ImageUrl).ToList()
            };
        }

        public async Task<FoundItemDto> CreateAsync(CreateFoundItemRequest request, int createdBy)
        {
            var entity = new FoundItem
            {
                Title = request.Title,
                Description = request.Description,
                FoundDate = request.FoundDate,
                FoundLocation = request.FoundLocation,
                CampusId = request.CampusId,
                CategoryId = request.CategoryId,
                CreatedBy = createdBy,
                StoredBy = request.StoredBy,
                Status = FoundItemStatus.Stored.ToString()
            };

            await _repo.AddAsync(entity);

            if (request.Images != null)
            {
                foreach (var file in request.Images)
                {
                    var url = await _imageService.UploadAsync(file);

                    await _imageRepo.AddAsync(new Image
                    {
                        FoundItemId = entity.FoundItemId,
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = createdBy
                    });
                }
            }

            return await GetByIdAsync(entity.FoundItemId);
        }

        public async Task<FoundItemDto> UpdateAsync(int id, UpdateFoundItemRequest request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Found item not found");

            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.FoundDate = request.FoundDate;
            entity.FoundLocation = request.FoundLocation;
            entity.CampusId = request.CampusId;
            entity.CategoryId = request.CategoryId;

            if (request.StoredBy.HasValue)
            {
                entity.StoredBy = request.StoredBy;
            }

            await _repo.UpdateAsync(entity);

            if (request.NewImages != null)
            {
                foreach (var file in request.NewImages)
                {
                    var url = await _imageService.UploadAsync(file);

                    await _imageRepo.AddAsync(new Image
                    {
                        FoundItemId = entity.FoundItemId,
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
            return await GetByIdAsync(entity.FoundItemId);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Found item not found");

            await _repo.DeleteAsync(entity);
        }

        public async Task<List<FoundItemDto>> GetByCampusAsync(int campusId)
        {
            var items = await _repo.GetByCampusAsync(campusId);
            return MapToDtoList(items);
        }

        public async Task<List<FoundItemDto>> GetByCategoryAsync(int categoryId)
        {
            var items = await _repo.GetByCategoryAsync(categoryId);
            return MapToDtoList(items);
        }

        public async Task<List<FoundItemDto>> SearchByTitleAsync(string title)
        {
            var items = await _repo.SearchByTitleAsync(title);
            return MapToDtoList(items);
        }

        private List<FoundItemDto> MapToDtoList(List<FoundItem> items)
        {
            return items.Select(f => new FoundItemDto
            {
                FoundItemId = f.FoundItemId,
                Title = f.Title,
                Description = f.Description,
                FoundDate = f.FoundDate,
                FoundLocation = f.FoundLocation,
                Status = f.Status,
                CampusId = f.CampusId,
                CampusName = f.Campus?.CampusName,
                CategoryId = f.CategoryId,
                CategoryName = f.Category?.CategoryName,
                CreatedBy = f.CreatedBy,
                StoredBy = f.StoredBy,
                ImageUrls = f.Images.Select(i => i.ImageUrl).ToList()
            }).ToList();
        }
    }
}