using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.FoundItemDTO;
using BLL.DTOs.LostItemDTO;
using BLL.DTOs.Security;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using BLL.DTOs; // Added to access ItemActionLogDto
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class FoundItemService : IFoundItemService
    {
        private readonly IFoundItemRepository _repo;
        private readonly IImageRepository _imageRepo;
        private readonly IImageService _imageService;
        private readonly IClaimRequestRepository _claimRequestRepository;
        private readonly IMatchingRepository _matchingRepository;
        private readonly ILostItemRepository _lostItemRepository;
        private readonly IItemActionLogService _itemActionLogService;
        private readonly INotificationService _notificationService;

        public FoundItemService(IFoundItemRepository repo, IImageRepository imageRepo, IImageService imageService, IClaimRequestRepository claimRequestRepository, IMatchingRepository matchingRepository, ILostItemRepository lostItemRepository, IItemActionLogService itemActionLogService, INotificationService notificationService)
        {
            _repo = repo;
            _imageRepo = imageRepo;
            _imageService = imageService;
            _claimRequestRepository = claimRequestRepository;
            _matchingRepository = matchingRepository;
            _lostItemRepository = lostItemRepository;
            _itemActionLogService = itemActionLogService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<FoundItemDto>> GetFoundItemsAsync(FoundItemFilterDto filter)
        {
            var items = await _repo.GetFoundItemsAsync(filter.CampusId, filter.Status);
            return MapToDtoList(items.ToList());
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

        public async Task<FoundItemDto?> GetFoundItemDetailsForUserAsync(int foundItemId)
        {
            var foundItem = await _repo.GetByIdAsync(foundItemId);
            if (foundItem == null) return null;

            return new FoundItemDto
            {
                FoundItemId = foundItem.FoundItemId,
                Title = foundItem.Title,
                Description = foundItem.Description,
                FoundDate = foundItem.FoundDate,
                FoundLocation = foundItem.FoundLocation,
                Status = foundItem.Status,
                CampusId = foundItem.CampusId,
                CampusName = foundItem.Campus?.CampusName,
                CategoryId = foundItem.CategoryId,
                CategoryName = foundItem.Category?.CategoryName,
                CreatedBy = foundItem.CreatedBy,
                StoredBy = foundItem.StoredBy,
                ImageUrls = foundItem.Images.Select(i => i.ImageUrl).ToList()
            };
        }

        public async Task<FoundItemDto> CreateAsync(CreateFoundItemRequest request, int createdBy, string initialStatus)
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
                StoredBy = (initialStatus == FoundItemStatus.Open.ToString() ? (int?)null : createdBy),
                Status = initialStatus ?? FoundItemStatus.Stored.ToString()
            };

            await _repo.AddAsync(entity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                FoundItemId = entity.FoundItemId,
                ActionType = "Created",
                ActionDetails = $"Found item '{entity.Title}' created with status '{entity.Status}'.",
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

            entity.Status = FoundItemStatus.Closed.ToString();
            await _repo.UpdateAsync(entity);
        }

        public async Task<List<FoundItemDto>> GetByCampusAsync(int campusId)
        {
            var items = await _repo.GetByCampusAsync(campusId);
            return MapToDtoList(items);
        }

        public async Task<List<FoundItemDto>> GetByCampusAsync(int campusId, string status)
        {
            var items = await _repo.GetByCampusAsync(campusId, status);
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

        public async Task<FoundItemDetailsDto> GetFoundItemDetailsAsync(int foundItemId)
        {
            var foundItem = await _repo.GetByIdAsync(foundItemId);
            if (foundItem == null)
            {
                throw new Exception("Found item not found.");
            }

            var approvedClaims = (await _claimRequestRepository.GetByFoundItemIdAsync(foundItemId))
                .Where(c => c.Status == "Returned")
                .Select(MapToClaimRequestDto)
                .ToList();

            var approvedMatches = (await _matchingRepository.GetMatchesForFoundItemAsync(foundItemId))
                .Where(m => m.MatchStatus == "Approved")
                .ToList();

            var approvedLostItems = new List<LostItemDto>();
            foreach (var match in approvedMatches)
            {
                if (match.LostItemId.HasValue)
                {
                    var lostItem = await _lostItemRepository.GetByIdAsync(match.LostItemId.Value);
                    if (lostItem != null)
                    {
                        approvedLostItems.Add(MapToLostItemDto(lostItem));
                    }
                }
            }

            var detailsDto = new FoundItemDetailsDto
            {
                FoundItemId = foundItem.FoundItemId,
                Title = foundItem.Title,
                Description = foundItem.Description,
                FoundDate = foundItem.FoundDate,
                FoundLocation = foundItem.FoundLocation,
                Status = foundItem.Status,
                CampusId = foundItem.CampusId,
                CampusName = foundItem.Campus?.CampusName,
                CategoryId = foundItem.CategoryId,
                CategoryName = foundItem.Category?.CategoryName,
                CreatedBy = foundItem.CreatedBy,
                StoredBy = foundItem.StoredBy,
                ImageUrls = foundItem.Images.Select(i => i.ImageUrl).ToList(),
                ApprovedClaimRequests = approvedClaims,
                ApprovedLostItems = approvedLostItems
            };

            return detailsDto;
        }

        private LostItemDto MapToLostItemDto(LostItem item)
        {
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

        private ClaimRequestDto MapToClaimRequestDto(ClaimRequest c)
        {
            return new ClaimRequestDto
            {
                ClaimId = c.ClaimId,
                ClaimDate = c.ClaimDate,
                Status = c.Status,
                FoundItemId = c.FoundItemId,
                FoundItemTitle = c.FoundItem?.Title,
                StudentId = c.StudentId,
                StudentName = c.Student?.FullName,
                Evidences = c.Evidences.Select(e => new EvidenceDto
                {
                    EvidenceId = e.EvidenceId,
                    Title = e.Title,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    ImageUrls = e.Images.Select(i => i.ImageUrl).ToList()
                }).ToList()
            };
        }

        public async Task<FoundItemDto> UpdateStatusAsync(int id, UpdateFoundItemStatusRequest request, int staffId)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Found item not found");

            string oldStatus = entity.Status;
            entity.Status = request.Status;
            if (request.Status == FoundItemStatus.Stored.ToString() && entity.StoredBy == null)
            {
                entity.StoredBy = staffId;
            }

            await _repo.UpdateAsync(entity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                FoundItemId = entity.FoundItemId,
                ActionType = "StatusUpdate",
                ActionDetails = $"Found item '{entity.Title}' status changed from '{oldStatus}' to '{entity.Status}'.",
                OldStatus = oldStatus,
                NewStatus = entity.Status,
                PerformedBy = staffId,
                CampusId = entity.CampusId // Assuming campus ID doesn't change
            });

            // If found item status changes to Returned, reject other claims and dismiss other matches
            if (request.Status == FoundItemStatus.Returned.ToString())
            {
                // Reject other ClaimRequests for this FoundItem
                var otherClaims = (await _claimRequestRepository.GetByFoundItemIdAsync(entity.FoundItemId))
                                    .Where(c => c.Status != ClaimStatus.Returned.ToString() && c.Status != ClaimStatus.Rejected.ToString()) 
                                    .ToList();

                foreach (var claim in otherClaims)
                {
                    string claimOldStatus = claim.Status;
                    claim.Status = ClaimStatus.Rejected.ToString();
                    await _claimRequestRepository.UpdateAsync(claim);

                    await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                    {
                        ClaimRequestId = claim.ClaimId,
                        FoundItemId = entity.FoundItemId,
                        ActionType = "StatusUpdate",
                        ActionDetails = $"Claim request '{claim.ClaimId}' for Found Item '{entity.Title}' auto-rejected due to Found Item being returned.",
                        OldStatus = claimOldStatus,
                        NewStatus = claim.Status,
                        PerformedBy = staffId, // Action performed by the staff who returned the FoundItem
                        CampusId = entity.CampusId
                    });
                    
                    // Notify student about rejected claim
                    if (claim.StudentId.HasValue)
                    {
                        string studentUserId = claim.StudentId.Value.ToString();
                        string message = $"Your claim request (ID: {claim.ClaimId}) for item '{entity.Title}' has been rejected because the item has been returned.";
                        try
                        {
                            await _notificationService.SendNotificationAsync(studentUserId, claim.ClaimId, claim.Status, message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send notification for rejected claim: {ex.Message}");
                        }
                    }
                }

                // Dismiss other ItemMatches for this FoundItem
                var otherMatches = (await _matchingRepository.GetMatchesForFoundItemAsync(entity.FoundItemId))
                                    .Where(m => m.Status != "Resolved" && m.MatchStatus != "Dismissed") // Exclude the match that might have led to this return
                                    .ToList();

                foreach (var match in otherMatches)
                {
                    string matchOldStatus = match.MatchStatus;
                    match.MatchStatus = "Dismissed";
                    match.Status = "Rejected"; // Or a more specific status for auto-dismissed
                    await _matchingRepository.UpdateMatchAsync(match);

                    await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                    {
                        FoundItemId = entity.FoundItemId,
                        LostItemId = match.LostItemId,
                        ActionType = "MatchDismissed",
                        ActionDetails = $"Match (ID: {match.MatchId}) for Found Item '{entity.Title}' auto-dismissed due to Found Item being returned.",
                        OldStatus = matchOldStatus,
                        NewStatus = match.MatchStatus,
                        PerformedBy = staffId,
                        CampusId = entity.CampusId
                    });
                }
            }

            return await GetByIdAsync(entity.FoundItemId);
        }
                public async Task<List<SecurityFoundItemDto>> GetOpenFoundItemsForSecurityOfficerAsync(int securityOfficerId)
                {
                    var items = await _repo.GetByCreatedByAndStatusAsync(securityOfficerId, FoundItemStatus.Open.ToString());
        
                    return items.Select(f => new SecurityFoundItemDto
                    {
                        FoundItemId = f.FoundItemId,
                        Title = f.Title,
                        Description = f.Description,
                        FoundDate = (DateTime)f.FoundDate,
                        FoundLocation = f.FoundLocation,
                        Status = f.Status,
                        CategoryId = f.CategoryId,
                        CategoryName = f.Category?.CategoryName,
                        ImageUrls = f.Images.Select(i => i.ImageUrl).ToList()
                    }).ToList();
                }
        
                        public async Task<IEnumerable<FoundItemDto>> GetByUserIdAsync(int userId)
                        {
                            var items = await _repo.GetByUserIdAsync(userId);
                            return MapToDtoList(items.ToList());
                        }
                
                        public async Task<FoundItemDto> UpdateFoundItemAsync(int id, UpdateFoundItemDTO foundItem)
                        {
                            var entity = await _repo.GetByIdAsync(id);
                            if (entity == null) throw new Exception("Found item not found");
                
                            if (!string.IsNullOrEmpty(foundItem.ItemName))
                            {
                                entity.Title = foundItem.ItemName;
                            }
                
                            if (!string.IsNullOrEmpty(foundItem.Description))
                            {
                                entity.Description = foundItem.Description;
                            }
                
                            if (foundItem.CategoryId.HasValue)
                            {
                                entity.CategoryId = foundItem.CategoryId.Value;
                            }
                
                            if (!string.IsNullOrEmpty(foundItem.LocationFound))
                            {
                                entity.FoundLocation = foundItem.LocationFound;
                            }
                            
                
                            await _repo.UpdateAsync(entity);
                
                            if (foundItem.Images != null)
                            {
                                foreach (var file in foundItem.Images)
                                {
                                    var url = await _imageService.UploadAsync(file);
                
                                    await _imageRepo.AddAsync(new Image
                                    {
                                        FoundItemId = entity.FoundItemId,
                                        ImageUrl = url,
                                        UploadedAt = DateTime.UtcNow,
                                        UploadedBy = entity.CreatedBy
                                    });
                                }
                            }
                            return await GetByIdAsync(entity.FoundItemId);
                        }
                    }
                }
                