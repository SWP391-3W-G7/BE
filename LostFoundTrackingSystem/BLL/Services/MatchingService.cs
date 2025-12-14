using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using System;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.DTOs.FoundItemDTO;
using BLL.DTOs.LostItemDTO;
using BLL.DTOs.MatchDTO;
using BLL.DTOs.ClaimRequestDTO; // Added
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly IMatchingRepository _matchingRepository;
        private readonly ILostItemRepository _lostItemRepository; // Already present
        private readonly IFoundItemRepository _foundItemRepository; // New injection
        private readonly IMatchHistoryRepository _matchHistoryRepository;
        private readonly INotificationService _notificationService;
        private readonly IItemActionLogService _itemActionLogService; // New injection
        private readonly IClaimRequestRepository _claimRequestRepository; // New injection

        public MatchingService(IMatchingRepository matchingRepository, ILostItemRepository lostItemRepository, IFoundItemRepository foundItemRepository, IMatchHistoryRepository matchHistoryRepository, INotificationService notificationService, IItemActionLogService itemActionLogService, IClaimRequestRepository claimRequestRepository)
        {
            _matchingRepository = matchingRepository;
            _lostItemRepository = lostItemRepository;
            _foundItemRepository = foundItemRepository;
            _matchHistoryRepository = matchHistoryRepository;
            _notificationService = notificationService;
            _itemActionLogService = itemActionLogService;
            _claimRequestRepository = claimRequestRepository;
        }

        public async Task FindAndCreateMatchesAsync(int lostItemId)
        {
            var lostItem = await _lostItemRepository.GetByIdAsync(lostItemId);
            if (lostItem == null)
            {
                // Or handle this case as you see fit
                throw new Exception("Lost item not found.");
            }
            int matchCount = 0;

            var potentialMatches = await _matchingRepository.GetPotentialMatchesAsync(lostItem);

            foreach (var foundItem in potentialMatches)
            {
                var existingMatch = await _matchingRepository.GetExistingMatchAsync(lostItem.LostItemId, foundItem.FoundItemId);
                if (existingMatch != null)
                {
                    // Match already exists, skip creating a new one
                    continue;
                }

                var itemMatch = new ItemMatch
                {
                    LostItemId = lostItem.LostItemId,
                    FoundItemId = foundItem.FoundItemId,
                    MatchStatus = "Matched", // System-generated match
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending" // Pending staff review
                };
                await _matchingRepository.AddMatchAsync(itemMatch);
                matchCount++;
            }
            if (matchCount > 0 && lostItem.CreatedBy.HasValue)
            {
                string userId = lostItem.CreatedBy.Value.ToString();
                string message = matchCount == 1
                    ? $"We found a potential match for your lost item '{lostItem.Title}'!"
                    : $"We found {matchCount} potential matches for your lost item '{lostItem.Title}'!";

                try
                {
                    await _notificationService.SendMatchNotificationAsync(
                        userId,
                        lostItemId,
                        message
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send match notification: {ex.Message}");
                }
            }
        }

        public async Task<IEnumerable<ItemMatchDto>> GetMatchesForFoundItemAsync(int foundItemId)
        {
            var matches = await _matchingRepository.GetMatchesForFoundItemAsync(foundItemId);
            var itemMatchDtos = new List<ItemMatchDto>();
            foreach (var match in matches)
            {
                itemMatchDtos.Add(await MapToItemMatchDto(match));
            }
            return itemMatchDtos;
        }

        public async Task<IEnumerable<ItemMatchDto>> GetMatchesForLostItemAsync(int lostItemId)
        {
            var matches = await _matchingRepository.GetMatchesForLostItemAsync(lostItemId);
            var itemMatchDtos = new List<ItemMatchDto>();
            foreach (var match in matches)
            {
                itemMatchDtos.Add(await MapToItemMatchDto(match));
            }
            return itemMatchDtos;
        }

        public async Task FindAndCreateMatchesForAllLostItemsAsync()
        {
            var allLostItems = await _lostItemRepository.GetAllAsync();
            foreach (var lostItem in allLostItems)
            {
                if (lostItem.Status == "Lost") 
                {
                    await FindAndCreateMatchesAsync(lostItem.LostItemId);
                }
            }
        }

        public async Task ConfirmMatchAsync(int matchId, int staffUserId)
        {
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.MatchStatus = "Approved";
                match.Status = "Resolved";
                await _matchingRepository.UpdateMatchAsync(match);

                if (match.LostItemId.HasValue)
                {
                    var lostItem = await _lostItemRepository.GetByIdAsync(match.LostItemId.Value);
                    if (lostItem != null)
                    {
                        string oldStatus = lostItem.Status;
                        lostItem.Status = LostItemStatus.Returned.ToString();
                        await _lostItemRepository.UpdateAsync(lostItem);

                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            LostItemId = lostItem.LostItemId,
                            ActionType = "StatusUpdate",
                            ActionDetails = $"Lost item '{lostItem.Title}' status changed from '{oldStatus}' to '{lostItem.Status}' due to match confirmation.",
                            OldStatus = oldStatus,
                            NewStatus = lostItem.Status,
                            PerformedBy = staffUserId,
                            CampusId = lostItem.CampusId
                        });
                    }
                }

                if (match.FoundItemId.HasValue)
                {
                    var foundItem = await _foundItemRepository.GetByIdAsync(match.FoundItemId.Value);
                    if (foundItem != null)
                    {
                        string oldStatus = foundItem.Status;
                        foundItem.Status = FoundItemStatus.Returned.ToString();
                        await _foundItemRepository.UpdateAsync(foundItem);

                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            FoundItemId = foundItem.FoundItemId,
                            ActionType = "StatusUpdate",
                            ActionDetails = $"Found item '{foundItem.Title}' status changed from '{oldStatus}' to '{foundItem.Status}' due to match confirmation.",
                            OldStatus = oldStatus,
                            NewStatus = foundItem.Status,
                            PerformedBy = staffUserId,
                            CampusId = foundItem.CampusId
                        });
                    }
                }

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Approved",
                    ActionDate = DateTime.UtcNow,
                    ActionBy = staffUserId
                });
            }
        }

        public async Task DismissMatchAsync(int matchId, int staffUserId)
        {
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.MatchStatus = "Dismissed";
                await _matchingRepository.UpdateMatchAsync(match);

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Dismissed",
                    ActionDate = DateTime.UtcNow,
                    ActionBy = staffUserId
                });
            }
        }

        public async Task ConflictMatchAsync(int matchId, int staffUserId)
        {
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match == null) throw new Exception("Match not found.");

            string oldStatus = match.MatchStatus;
            match.MatchStatus = "Conflicted"; // New status
            await _matchingRepository.UpdateMatchAsync(match);

            await _matchHistoryRepository.AddAsync(new MatchHistory
            {
                MatchId = matchId,
                Action = "Conflicted",
                ActionDate = DateTime.UtcNow,
                ActionBy = staffUserId
            });

            // Log to ItemActionLog for the associated FoundItem and LostItem
            if (match.FoundItemId.HasValue)
            {
                var foundItem = await _foundItemRepository.GetByIdAsync(match.FoundItemId.Value);
                if (foundItem != null)
                {
                    await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                    {
                        FoundItemId = foundItem.FoundItemId,
                        ActionType = "MatchConflict",
                        ActionDetails = $"Match (ID: {match.MatchId}) for Found Item '{foundItem.Title}' marked as Conflicted.",
                        OldStatus = oldStatus,
                        NewStatus = match.MatchStatus,
                        PerformedBy = staffUserId,
                        CampusId = foundItem.CampusId
                    });
                }
            }

            if (match.LostItemId.HasValue)
            {
                var lostItem = await _lostItemRepository.GetByIdAsync(match.LostItemId.Value);
                if (lostItem != null)
                {
                    await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                    {
                        LostItemId = lostItem.LostItemId,
                        ActionType = "MatchConflict",
                        ActionDetails = $"Match (ID: {match.MatchId}) for Lost Item '{lostItem.Title}' marked as Conflicted.",
                        OldStatus = oldStatus,
                        NewStatus = match.MatchStatus,
                        PerformedBy = staffUserId,
                        CampusId = lostItem.CampusId
                    });
                }
            }
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

        private async Task<ItemMatchDto> MapToItemMatchDto(ItemMatch match)
        {
            var foundItemClaims = new List<ClaimRequestDto>();
            if (match.FoundItem?.FoundItemId != null)
            {
                var claims = await _claimRequestRepository.GetByFoundItemIdAsync(match.FoundItem.FoundItemId);
                foundItemClaims = claims.Select(c => new ClaimRequestDto
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
                }).ToList();
            }

            return new ItemMatchDto
            {
                MatchId = match.MatchId,
                MatchStatus = match.MatchStatus,
                CreatedAt = match.CreatedAt,
                Status = match.Status,
                LostItemId = match.LostItemId,
                FoundItemId = match.FoundItemId,
                CreatedBy = match.CreatedBy,
                CreatedByNavigation = match.CreatedByNavigation != null ? new UserDto
                {
                    UserId = match.CreatedByNavigation.UserId,
                    Username = match.CreatedByNavigation.Username,
                    Email = match.CreatedByNavigation.Email,
                    FullName = match.CreatedByNavigation.FullName,
                    RoleId = match.CreatedByNavigation.RoleId.Value,
                    Status = match.CreatedByNavigation.Status,
                    CampusId = match.CreatedByNavigation.CampusId,
                    PhoneNumber = match.CreatedByNavigation.PhoneNumber,
                    RoleName = match.CreatedByNavigation.Role?.RoleName,
                    CampusName = match.CreatedByNavigation.Campus?.CampusName
                } : null,
                FoundItem = match.FoundItem != null ? new FoundItemDto
                {
                    FoundItemId = match.FoundItem.FoundItemId,
                    Title = match.FoundItem.Title,
                    Description = match.FoundItem.Description,
                    FoundDate = match.FoundItem.FoundDate,
                    FoundLocation = match.FoundItem.FoundLocation,
                    Status = match.FoundItem.Status,
                    CampusId = match.FoundItem.CampusId,
                    CampusName = match.FoundItem.Campus?.CampusName,
                    CategoryId = match.FoundItem.CategoryId,
                    CategoryName = match.FoundItem.Category?.CategoryName,
                    ImageUrls = match.FoundItem.Images.Select(i => i.ImageUrl).ToList(),
                    ClaimRequests = foundItemClaims
                } : null,
                LostItem = match.LostItem != null ? new LostItemDto
                {
                    LostItemId = match.LostItem.LostItemId,
                    Title = match.LostItem.Title,
                    Description = match.LostItem.Description,
                    LostDate = match.LostItem.LostDate,
                    LostLocation = match.LostItem.LostLocation,
                    Status = match.LostItem.Status,
                    CampusId = match.LostItem.CampusId,
                    CampusName = match.LostItem.Campus?.CampusName,
                    CategoryId = match.LostItem.CategoryId,
                    CategoryName = match.LostItem.Category?.CategoryName,
                    ImageUrls = match.LostItem.Images.Select(i => i.ImageUrl).ToList()
                } : null
            };
        }
    }
}
