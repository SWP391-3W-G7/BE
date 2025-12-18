using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.LostItemDTO;
using BLL.DTOs.Paging;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using BLL.DTOs; 

namespace BLL.Services 
{
    public class LostItemService : ILostItemService
    {
        private readonly ILostItemRepository _repo;
        private readonly IImageRepository _imageRepo;
        private readonly IImageService _imageService;
        private readonly IMatchingService _matchingService;
        private readonly IMatchingRepository _matchRepo;
        private readonly IReturnRecordRepository _returnRecordRepo;
        private readonly IItemActionLogService _itemActionLogService;

        public LostItemService(ILostItemRepository repo, IImageRepository imageRepo, IImageService imageService, IMatchingService matchingService, IMatchingRepository matchRepo, IReturnRecordRepository returnRecordRepo, IItemActionLogService itemActionLogService)
        {
            _repo = repo;
            _imageRepo = imageRepo;
            _imageService = imageService;
            _matchingService = matchingService;
            _matchRepo = matchRepo;
            _returnRecordRepo = returnRecordRepo;
            _itemActionLogService = itemActionLogService;
        }
        
        public async Task<LostItemDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;

            var logs = await _itemActionLogService.GetLogsByLostItemIdAsync(item.LostItemId);

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
                ImageUrls = item.Images.Select(i => i.ImageUrl).ToList(),
                ActionLogs = logs
            };
        }
        public async Task<LostItemDto> CreateAsync(CreateLostItemRequest request, int createdBy)
        {
            var entity = new LostItem
            {
                Title = request.Title,
                Description = request.Description,
                LostDate = request.LostDate,
                LostLocation = request.LostLocation,
                CampusId = request.CampusId,
                CategoryId = request.CategoryId,
                CreatedBy = createdBy,
                Status = LostItemStatus.Lost.ToString()
            };

            await _repo.AddAsync(entity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                LostItemId = entity.LostItemId,
                ActionType = "Created",
                ActionDetails = $"Lost item '{entity.Title}' created with status '{entity.Status}'.",
                NewStatus = entity.Status,
                PerformedBy = createdBy,
                CampusId = entity.CampusId
            });

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
                        UploadedBy = createdBy
                    });
                }
            }

            // Trigger auto-matching after a new lost item is created
            await _matchingService.FindAndCreateMatchesAsync(entity.LostItemId);

            return await GetByIdAsync(entity.LostItemId);
        }
        public async Task<LostItemDto> UpdateAsync(int id, UpdateLostItemRequest request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Lost item not found");

            string oldStatus = entity.Status;
            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.LostDate = request.LostDate;
            entity.LostLocation = request.LostLocation;
            entity.CampusId = request.CampusId;
            entity.CategoryId = request.CategoryId;

            await _repo.UpdateAsync(entity);

            // Log update action if status changed
            if (oldStatus != entity.Status)
            {
                await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                {
                    LostItemId = entity.LostItemId,
                    ActionType = "Updated",
                    ActionDetails = $"Lost item '{entity.Title}' updated. Status changed from '{oldStatus}' to '{entity.Status}'.",
                    OldStatus = oldStatus,
                    NewStatus = entity.Status,
                    PerformedBy = entity.CreatedBy, // Assuming the creator is the one updating, or pass a specific updater ID
                    CampusId = entity.CampusId
                });
            } else {
                 await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                {
                    LostItemId = entity.LostItemId,
                    ActionType = "Updated",
                    ActionDetails = $"Lost item '{entity.Title}' updated.",
                    PerformedBy = entity.CreatedBy,
                    CampusId = entity.CampusId
                });
            }


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

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                LostItemId = entity.LostItemId,
                ActionType = "Deleted",
                ActionDetails = $"Lost item '{entity.Title}' deleted.",
                OldStatus = entity.Status,
                PerformedBy = entity.CreatedBy,
                CampusId = entity.CampusId
            });
        }
        public async Task<List<LostItemDto>> GetByCampusAsync(int campusId)
        {
            var items = await _repo.GetByCampusAsync(campusId);
            return await MapToDtoList(items);
        }

        public async Task<List<LostItemDto>> GetByCategoryAsync(int categoryId)
        {
            var items = await _repo.GetByCategoryAsync(categoryId);
            return await MapToDtoList(items);
        }

        public async Task<List<LostItemDto>> SearchByTitleAsync(string title)
        {
            var items = await _repo.SearchByTitleAsync(title);
            return await MapToDtoList(items);
        }
        public async Task<List<LostItemDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();

            return await MapToDtoList(items);
        }

        public async Task<PagedResponse<LostItemDto>> GetAllPagingAsync(LostItemFilterDto filter, PagingParameters pagingParameters)
        {
            var (items, totalCount) = await _repo.GetLostItemsPagingAsync(filter.CampusId, filter.Status, pagingParameters.PageNumber, pagingParameters.PageSize);
            
            var dtoList = await MapToDtoList(items);

            return new PagedResponse<LostItemDto>(dtoList, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<List<LostItemDto>> GetMyLostItemsAsync(int userId)
        {
            var items = await _repo.GetByCreatedByAsync(userId);
            return await MapToDtoList(items);
        }

        private async Task<List<LostItemDto>> MapToDtoList(List<LostItem> items)
        {
            var dtoList = new List<LostItemDto>();
            foreach (var item in items)
            {
                var logs = await _itemActionLogService.GetLogsByLostItemIdAsync(item.LostItemId);
                dtoList.Add(new LostItemDto
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
                    ImageUrls = item.Images.Select(i => i.ImageUrl).ToList(),
                    ActionLogs = logs
                });
            }
            return dtoList;
        }

        public async Task<LostItemDto> UpdateStatusAsync(int lostItemId, UpdateLostItemStatusRequest request, int staffId)
        {
            var entity = await _repo.GetByIdAsync(lostItemId);
            if (entity == null) throw new Exception("Lost item not found");

            string oldStatus = entity.Status;
            entity.Status = request.Status;

            if (entity.Status.ToString() == LostItemStatus.Returned.ToString())
            {
                var match = (await _matchRepo.GetMatchesForLostItemAsync(lostItemId)).FirstOrDefault();
                if (match == null)
                    throw new Exception("No match found for this lost item.");

                var returnRecord = new ReturnRecord
                {
                    LostItemId = lostItemId,
                    FoundItemId = match.FoundItemId,
                    ReceiverId = entity.CreatedBy,
                    StaffUserId = staffId,
                    ReturnDate = DateTime.UtcNow
                };
                await _returnRecordRepo.AddAsync(returnRecord);
            }

            await _repo.UpdateAsync(entity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                LostItemId = entity.LostItemId,
                ActionType = "StatusUpdate",
                ActionDetails = $"Lost item '{entity.Title}' status changed from '{oldStatus}' to '{entity.Status}'.",
                OldStatus = oldStatus,
                NewStatus = entity.Status,
                PerformedBy = staffId,
                CampusId = entity.CampusId
            });

            return await GetByIdAsync(entity.LostItemId);
        }
    }
}
