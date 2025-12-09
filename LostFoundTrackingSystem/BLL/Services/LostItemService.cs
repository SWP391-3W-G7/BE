using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.LostItemDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;

namespace BLL.Services 
{
    public class LostItemService : ILostItemService
    {
        private readonly ILostItemRepository _repo;
        private readonly IImageRepository _imageRepo;
        private readonly IImageService _imageService;

        public LostItemService(ILostItemRepository repo, IImageRepository imageRepo, IImageService imageService)
        {
            _repo = repo;
            _imageRepo = imageRepo;
            _imageService = imageService;
        }
        
        public async Task<LostItemDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;

            return new LostItemDto
            {
                LostItemId = item.LostItemId,
                Title = item.Title,
                Description = item.Description,
                LostDate = item.LostDate,
                LostLocation = item.LostLocation,
                Status = item.Status,
                CampusId = item.CampusId,
                CampusName = item.Campus?.CampusName,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.CategoryName,
                ImageUrls = item.Images.Select(i => i.ImageUrl).ToList()
            };
        }
        public async Task<LostItemDto> CreateAsync(CreateLostItemRequest request)
        {
            var entity = new LostItem
            {
                Title = request.Title,
                Description = request.Description,
                LostDate = request.LostDate,
                LostLocation = request.LostLocation,
                CampusId = request.CampusId,
                CategoryId = request.CategoryId,
                CreatedBy = request.CreatedBy,
                Status = LostItemStatus.Lost.ToString()
            };

            await _repo.AddAsync(entity);

            if (request.Images != null)
            {
                foreach (var file in request.Images)
                {
                    var url = await _imageService.UploadAsync(file);

                    await _imageRepo.AddAsync(new Image
                    {
                        LostItemId = entity.LostItemId,
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = request.CreatedBy
                    });
                }
            }

            return await GetByIdAsync(entity.LostItemId);
        }
        public async Task<LostItemDto> UpdateAsync(int id, UpdateLostItemRequest request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Lost item not found");

            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.LostDate = request.LostDate;
            entity.LostLocation = request.LostLocation;
            entity.CampusId = request.CampusId;
            entity.CategoryId = request.CategoryId;

            await _repo.UpdateAsync(entity);

            if (request.NewImages != null)
            {
                foreach (var file in request.NewImages)
                {
                    var url = await _imageService.UploadAsync(file);

                    await _imageRepo.AddAsync(new Image
                    {
                        LostItemId = entity.LostItemId,
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
            return await GetByIdAsync(entity.LostItemId);
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Lost item not found");

            await _repo.DeleteAsync(entity);
        }
        public async Task<List<LostItemDto>> GetByCampusAsync(int campusId)
        {
            var items = await _repo.GetByCampusAsync(campusId);
            return MapToDtoList(items);
        }

        public async Task<List<LostItemDto>> GetByCategoryAsync(int categoryId)
        {
            var items = await _repo.GetByCategoryAsync(categoryId);
            return MapToDtoList(items);
        }

        public async Task<List<LostItemDto>> SearchByTitleAsync(string title)
        {
            var items = await _repo.SearchByTitleAsync(title);
            return MapToDtoList(items);
        }
        public async Task<List<LostItemDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();

            return MapToDtoList(items);
        }

        private List<LostItemDto> MapToDtoList(List<LostItem> items)
        {
            return items.Select(l => new LostItemDto
            {
                LostItemId = l.LostItemId,
                Title = l.Title,
                Description = l.Description,
                LostDate = l.LostDate,
                LostLocation = l.LostLocation,
                Status = l.Status,
                CampusId = l.CampusId,
                CampusName = l.Campus?.CampusName,
                CategoryId = l.CategoryId,
                CategoryName = l.Category?.CategoryName,
                ImageUrls = l.Images.Select(i => i.ImageUrl).ToList()
            }).ToList();
        }
    }
}
