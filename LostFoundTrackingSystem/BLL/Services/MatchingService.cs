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
using BLL.DTOs.Paging;
using Microsoft.Extensions.Logging; // Added

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
        private readonly ILogger<MatchingService> _logger; // Added


        public MatchingService(IMatchingRepository matchingRepository, ILostItemRepository lostItemRepository, IFoundItemRepository foundItemRepository, IMatchHistoryRepository matchHistoryRepository, INotificationService notificationService, IItemActionLogService itemActionLogService, IClaimRequestRepository claimRequestRepository, ILogger<MatchingService> logger)
        {
            _matchingRepository = matchingRepository;
            _lostItemRepository = lostItemRepository;
            _foundItemRepository = foundItemRepository;
            _matchHistoryRepository = matchHistoryRepository;
            _notificationService = notificationService;
            _itemActionLogService = itemActionLogService;
            _claimRequestRepository = claimRequestRepository;
            _logger = logger; // Initialized
        }

        public async Task FindAndCreateMatchesAsync(int lostItemId)
        {
            _logger.LogInformation("Attempting to find and create matches for lost item ID: {LostItemId}", lostItemId);
            try
            {
                var lostItem = await _lostItemRepository.GetByIdAsync(lostItemId);
                // The next line `var foundItem_ = await _foundItemRepository.GetByIdAsync(lostItemId);` is problematic.
                // It uses lostItemId as foundItemId, which is likely incorrect. It's not used in this method, so removing it.
                // var foundItem_ = await _foundItemRepository.GetByIdAsync(lostItemId);

                if (lostItem == null)
                {
                    _logger.LogWarning("Lost item with ID {LostItemId} not found. Skipping match creation.", lostItemId);
                    return; // Or throw an exception if this is an unexpected state
                }
                _logger.LogInformation("Processing lost item {LostItemTitle} (ID: {LostItemId}). Status: {LostItemStatus}", lostItem.Title, lostItemId, lostItem.Status);

                int matchCount = 0;

                var potentialMatches = await _matchingRepository.GetPotentialMatchesAsync(lostItem);
                _logger.LogInformation("Found {PotentialMatchesCount} potential matches for lost item {LostItemId}.", potentialMatches.Count(), lostItemId);

                foreach (var foundItem in potentialMatches)
                {
                    _logger.LogInformation("Checking potential match: LostItemId {LostItemId} with FoundItemId {FoundItemId}.", lostItemId, foundItem.FoundItemId);
                    var existingMatch = await _matchingRepository.GetExistingMatchAsync(lostItem.LostItemId, foundItem.FoundItemId);
                    if (existingMatch != null)
                    {
                        _logger.LogInformation("An existing match (MatchId: {MatchId}) for lost item {LostItemId} and found item {FoundItemId} already exists. Skipping.", existingMatch.MatchId, lostItemId, foundItem.FoundItemId);
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
                    _logger.LogInformation("New match created (MatchId: {MatchId}) for lost item {LostItemId} and found item {FoundItemId}.", itemMatch.MatchId, lostItemId, foundItem.FoundItemId);


                    // Auto create claim request
                    if (lostItem.CreatedBy.HasValue)
                    {
                        _logger.LogInformation("Attempting to auto-create claim request for lost item {LostItemId} due to new match. CreatedBy: {CreatedBy}", lostItemId, lostItem.CreatedBy.Value);
                        var claimRequest = new ClaimRequest
                        {
                            FoundItemId = foundItem.FoundItemId,
                            LostItemId = lostItem.LostItemId,
                            StudentId = lostItem.CreatedBy.Value,
                            ClaimDate = DateTime.UtcNow,
                            Status = ClaimStatus.Pending.ToString(),
                            Priority = (int)ClaimPriority.High
                        };
                        await _claimRequestRepository.AddAsync(claimRequest);
                        _logger.LogInformation("Claim request {ClaimId} auto-created for lost item {LostItemId} and found item {FoundItemId}.", claimRequest.ClaimId, lostItemId, foundItem.FoundItemId);

                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            FoundItemId = foundItem.FoundItemId,
                            ClaimRequestId = claimRequest.ClaimId,
                            ActionType = "Created",
                            ActionDetails = $"Claim auto-created from match. Priority: High. Status: {claimRequest.Status}.",
                            NewStatus = claimRequest.Status,
                            PerformedBy = lostItem.CreatedBy.Value,
                            CampusId = foundItem.CampusId
                        });
                    }
                }
                if (matchCount > 0 && lostItem.CreatedBy.HasValue)
                {
                    string userId = lostItem.CreatedBy.Value.ToString();
                    string message = matchCount == 1
                        ? $"We found a potential match for your lost item '{lostItem.Title}'!" // Removed foundItem_.Title
                        : $"We found {matchCount} potential matches for your lost item '{lostItem.Title}'!"; // Removed foundItem_.Title

                    try
                    {
                        _logger.LogInformation("Sending match notification to user {UserId} for lost item {LostItemId}.", userId, lostItemId);
                        await _notificationService.SendMatchNotificationAsync(
                            userId,
                            lostItemId,
                            message
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send match notification for lost item {LostItemId} to user {UserId}.", lostItemId, userId);
                    }
                }
                _logger.LogInformation("Finished finding and creating matches for lost item ID: {LostItemId}. Total new matches: {MatchCount}.", lostItemId, matchCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding and creating matches for lost item ID: {LostItemId}", lostItemId);
                throw; // Re-throw the exception after logging
            }
        }

        public async Task<IEnumerable<ItemMatchDto>> GetMatchesForFoundItemAsync(int foundItemId)
        {
            _logger.LogInformation("Fetching matches for found item ID: {FoundItemId}", foundItemId);
            var matches = await _matchingRepository.GetMatchesForFoundItemAsync(foundItemId);
            var itemMatchDtos = new List<ItemMatchDto>();
            foreach (var match in matches)
            {
                itemMatchDtos.Add(await MapToItemMatchDto(match));
            }
            _logger.LogInformation("Found {Count} matches for found item ID: {FoundItemId}", itemMatchDtos.Count, foundItemId);
            return itemMatchDtos;
        }

        public async Task<IEnumerable<ItemMatchDto>> GetMatchesForLostItemAsync(int lostItemId)
        {
            _logger.LogInformation("Fetching matches for lost item ID: {LostItemId}", lostItemId);
            var matches = await _matchingRepository.GetMatchesForLostItemAsync(lostItemId);
            var itemMatchDtos = new List<ItemMatchDto>();
            foreach (var match in matches)
            {
                itemMatchDtos.Add(await MapToItemMatchDto(match));
            }
            _logger.LogInformation("Found {Count} matches for lost item ID: {LostItemId}", itemMatchDtos.Count, lostItemId);
            return itemMatchDtos;
        }

        public async Task FindAndCreateMatchesForAllLostItemsAsync()
        {
            _logger.LogInformation("Starting to find and create matches for all lost items.");
            try
            {
                var allLostItems = await _lostItemRepository.GetAllAsync();
                _logger.LogInformation("Found {Count} lost items to process.", allLostItems.Count());
                foreach (var lostItem in allLostItems)
                {
                    if (lostItem.Status == "Lost")
                    {
                        _logger.LogInformation("Processing lost item {LostItemId} with status 'Lost'.", lostItem.LostItemId);
                        await FindAndCreateMatchesAsync(lostItem.LostItemId);
                    }
                    else
                    {
                        _logger.LogInformation("Skipping lost item {LostItemId} with status {Status} as it's not 'Lost'.", lostItem.LostItemId, lostItem.Status);
                    }
                }
                _logger.LogInformation("Finished finding and creating matches for all lost items.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding and creating matches for all lost items.");
                throw;
            }
        }

        public async Task<MatchOperationResponseDto> ConfirmMatchAsync(int matchId, int staffUserId)
        {
            _logger.LogInformation("Confirming match {MatchId} by staff user {StaffUserId}.", matchId, staffUserId);
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.MatchStatus = "Approved";
                match.Status = "Approved";
                await _matchingRepository.UpdateMatchAsync(match);
                _logger.LogInformation("Match {MatchId} status updated to Approved/Resolved.", matchId);

                LostItemDto? lostItemDto = null;
                if (match.LostItemId.HasValue)
                {
                    var lostItem = await _lostItemRepository.GetByIdAsync(match.LostItemId.Value);
                    if (lostItem != null)
                    {
                        string oldStatus = lostItem.Status;
                        lostItem.Status = LostItemStatus.Returned.ToString();
                        await _lostItemRepository.UpdateAsync(lostItem);
                        _logger.LogInformation("Lost item {LostItemId} status updated to Returned.", lostItem.LostItemId);

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
                        lostItemDto = MapToLostItemDto(lostItem);
                    }
                }

                FoundItemDto? foundItemDto = null;
                if (match.FoundItemId.HasValue)
                {
                    var foundItem = await _foundItemRepository.GetByIdAsync(match.FoundItemId.Value);
                    if (foundItem != null)
                    {
                        string oldStatus = foundItem.Status;
                        foundItem.Status = FoundItemStatus.Returned.ToString();
                        await _foundItemRepository.UpdateAsync(foundItem);
                        _logger.LogInformation("Found item {FoundItemId} status updated to Returned.", foundItem.FoundItemId);

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
                        foundItemDto = MapToFoundItemDto(foundItem);
                    }
                }

                var otherClaims = await _claimRequestRepository.GetByFoundItemIdAsync(match.FoundItemId.Value);
                foreach (var otherClaim in otherClaims)
                {
                    if (otherClaim.Status == ClaimStatus.Pending.ToString() || otherClaim.Status == ClaimStatus.Conflicted.ToString())
                    {
                        otherClaim.Status = ClaimStatus.Rejected.ToString();
                        await _claimRequestRepository.UpdateAsync(otherClaim);

                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            ClaimRequestId = otherClaim.ClaimId,
                            FoundItemId = otherClaim.FoundItemId,
                            ActionType = "StatusUpdate",
                            ActionDetails = $"Claim request '{otherClaim.ClaimId}' for Found Item '{match.FoundItem?.Title}' auto-rejected because a match was confirmed.",
                            OldStatus = "Pending/Conflicted",
                            NewStatus = otherClaim.Status,
                            PerformedBy = staffUserId,
                            CampusId = match.FoundItem?.CampusId
                        });

                        if (otherClaim.StudentId.HasValue)
                        {
                            string rejectedStudentUserId = otherClaim.StudentId.Value.ToString();
                            string rejectedMessage = $"Your claim request (ID: {otherClaim.ClaimId}) for item '{match.FoundItem?.Title}' has been rejected because a match was confirmed.";
                            try
                            {
                                await _notificationService.SendNotificationAsync(rejectedStudentUserId, otherClaim.ClaimId, otherClaim.Status, rejectedMessage);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send notification for rejected claim: {ClaimId}", otherClaim.ClaimId);
                            }
                        }
                    }
                }

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Approved",
                    ActionDate = DateTime.UtcNow,
                    ActionBy = staffUserId
                });
                _logger.LogInformation("Match history added for approved match {MatchId}.", matchId);
                return new MatchOperationResponseDto { IsSuccess = true, Message = $"Match {matchId} confirmed successfully.", MatchId = matchId, LostItem = lostItemDto, FoundItem = foundItemDto };
            }
            else
            {
                _logger.LogWarning("Match {MatchId} not found during confirmation attempt.", matchId);
                return new MatchOperationResponseDto { IsSuccess = false, Message = $"Match {matchId} not found.", MatchId = matchId };
            }
        }

        public async Task<MatchOperationResponseDto> DismissMatchAsync(int matchId, int staffUserId)
        {
            _logger.LogInformation("Dismissing match {MatchId} by staff user {StaffUserId}.", matchId, staffUserId);
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.MatchStatus = "Dismissed";
                await _matchingRepository.UpdateMatchAsync(match);
                _logger.LogInformation("Match {MatchId} status updated to Dismissed.", matchId);

                LostItemDto? lostItemDto = null;
                if (match.LostItemId.HasValue)
                {
                    var lostItem = await _lostItemRepository.GetByIdAsync(match.LostItemId.Value);
                    if (lostItem != null)
                    {
                        lostItemDto = MapToLostItemDto(lostItem);
                    }
                }

                FoundItemDto? foundItemDto = null;
                if (match.FoundItemId.HasValue)
                {
                    var foundItem = await _foundItemRepository.GetByIdAsync(match.FoundItemId.Value);
                    if (foundItem != null)
                    {
                        foundItemDto = MapToFoundItemDto(foundItem);
                    }
                }

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Dismissed",
                    ActionDate = DateTime.UtcNow,
                    ActionBy = staffUserId
                });
                _logger.LogInformation("Match history added for dismissed match {MatchId}.", matchId);
                return new MatchOperationResponseDto { IsSuccess = true, Message = $"Match {matchId} dismissed successfully.", MatchId = matchId, LostItem = lostItemDto, FoundItem = foundItemDto };
            }
            else
            {
                _logger.LogWarning("Match {MatchId} not found during dismissal attempt.", matchId);
                return new MatchOperationResponseDto { IsSuccess = false, Message = $"Match {matchId} not found.", MatchId = matchId };
            }
        }

        public async Task<MatchOperationResponseDto> ReturnMatchAsync(int matchId, int staffUserId)
        {
            _logger.LogInformation("Returning match {MatchId} by staff user {StaffUserId}.", matchId, staffUserId);
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.MatchStatus = "Returned";
                match.Status = "Returned";
                await _matchingRepository.UpdateMatchAsync(match);
                _logger.LogInformation("Match {MatchId} status updated to Returned.", matchId);

                LostItemDto? lostItemDto = null;
                if (match.LostItemId.HasValue)
                {
                    var lostItem = await _lostItemRepository.GetByIdAsync(match.LostItemId.Value);
                    if (lostItem != null)
                    {
                        string oldStatus = lostItem.Status;
                        lostItem.Status = LostItemStatus.Returned.ToString();
                        await _lostItemRepository.UpdateAsync(lostItem);
                        _logger.LogInformation("Lost item {LostItemId} status updated to Returned.", lostItem.LostItemId);

                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            LostItemId = lostItem.LostItemId,
                            ActionType = "StatusUpdate",
                            ActionDetails = $"Lost item '{lostItem.Title}' status changed from '{oldStatus}' to '{lostItem.Status}' due to match return.",
                            OldStatus = oldStatus,
                            NewStatus = lostItem.Status,
                            PerformedBy = staffUserId,
                            CampusId = lostItem.CampusId
                        });
                        lostItemDto = MapToLostItemDto(lostItem);
                    }
                }

                FoundItemDto? foundItemDto = null;
                if (match.FoundItemId.HasValue)
                {
                    var foundItem = await _foundItemRepository.GetByIdAsync(match.FoundItemId.Value);
                    if (foundItem != null)
                    {
                        string oldStatus = foundItem.Status;
                        foundItem.Status = FoundItemStatus.Returned.ToString();
                        await _foundItemRepository.UpdateAsync(foundItem);
                        _logger.LogInformation("Found item {FoundItemId} status updated to Returned.", foundItem.FoundItemId);

                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            FoundItemId = foundItem.FoundItemId,
                            ActionType = "StatusUpdate",
                            ActionDetails = $"Found item '{foundItem.Title}' status changed from '{oldStatus}' to '{foundItem.Status}' due to match return.",
                            OldStatus = oldStatus,
                            NewStatus = foundItem.Status,
                            PerformedBy = staffUserId,
                            CampusId = foundItem.CampusId
                        });
                        foundItemDto = MapToFoundItemDto(foundItem);
                    }
                }

                var otherClaims = await _claimRequestRepository.GetByFoundItemIdAsync(match.FoundItemId.Value);
                foreach (var otherClaim in otherClaims)
                {
                    if (otherClaim.Status == ClaimStatus.Pending.ToString() || otherClaim.Status == ClaimStatus.Conflicted.ToString())
                    {
                        otherClaim.Status = ClaimStatus.Rejected.ToString();
                        await _claimRequestRepository.UpdateAsync(otherClaim);

                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            ClaimRequestId = otherClaim.ClaimId,
                            FoundItemId = otherClaim.FoundItemId,
                            ActionType = "StatusUpdate",
                            ActionDetails = $"Claim request '{otherClaim.ClaimId}' for Found Item '{match.FoundItem?.Title}' auto-rejected because a match was returned.",
                            OldStatus = "Pending/Conflicted",
                            NewStatus = otherClaim.Status,
                            PerformedBy = staffUserId,
                            CampusId = match.FoundItem?.CampusId
                        });

                        if (otherClaim.StudentId.HasValue)
                        {
                            string rejectedStudentUserId = otherClaim.StudentId.Value.ToString();
                            string rejectedMessage = $"Your claim request (ID: {otherClaim.ClaimId}) for item '{match.FoundItem?.Title}' has been rejected because a match was returned.";
                            try
                            {
                                await _notificationService.SendNotificationAsync(rejectedStudentUserId, otherClaim.ClaimId, otherClaim.Status, rejectedMessage);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send notification for rejected claim: {ClaimId}", otherClaim.ClaimId);
                            }
                        }
                    }
                }

                await _matchHistoryRepository.AddAsync(new MatchHistory
                {
                    MatchId = matchId,
                    Action = "Returned",
                    ActionDate = DateTime.UtcNow,
                    ActionBy = staffUserId
                });
                _logger.LogInformation("Match history added for returned match {MatchId}.", matchId);
                return new MatchOperationResponseDto { IsSuccess = true, Message = $"Match {matchId} returned successfully.", MatchId = matchId, LostItem = lostItemDto, FoundItem = foundItemDto };
            }
            else
            {
                _logger.LogWarning("Match {MatchId} not found during return attempt.", matchId);
                return new MatchOperationResponseDto { IsSuccess = false, Message = $"Match {matchId} not found.", MatchId = matchId };
            }
        }


        public async Task ConflictMatchAsync(int matchId, int staffUserId)
        {
            _logger.LogInformation("Marking match {MatchId} as conflicted by staff user {StaffUserId}.", matchId, staffUserId);
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match == null)
            {
                _logger.LogWarning("Match {MatchId} not found during conflict marking attempt.", matchId);
                throw new Exception("Match not found.");
            }

            string oldStatus = match.MatchStatus;
            match.MatchStatus = "Conflicted"; // New status
            await _matchingRepository.UpdateMatchAsync(match);
            _logger.LogInformation("Match {MatchId} status updated to Conflicted.", matchId);

            await _matchHistoryRepository.AddAsync(new MatchHistory
            {
                MatchId = matchId,
                Action = "Conflicted",
                ActionDate = DateTime.UtcNow,
                ActionBy = staffUserId
            });
            _logger.LogInformation("Match history added for conflicted match {MatchId}.", matchId);

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
                    _logger.LogInformation("Item action log added for FoundItem {FoundItemId} related to match {MatchId}.", foundItem.FoundItemId, matchId);
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
                    _logger.LogInformation("Item action log added for LostItem {LostItemId} related to match {MatchId}.", lostItem.LostItemId, matchId);
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

        public async Task<ItemMatchDto> GetMatchDetailsByIdAsync(int matchId)
        {
            var match = await _matchingRepository.GetMatchByIdAsync(matchId);
            if (match == null) return null;
            return await MapToItemMatchDto(match);
        }
        public async Task<PagedResponse<ItemMatchDto>> GetAllMatchesPagingAsync(PagingParameters pagingParameters)
        {
            var (items, totalCount) = await _matchingRepository.GetMatchesPagingAsync(null, pagingParameters.PageNumber, pagingParameters.PageSize);

            var dtoList = new List<ItemMatchDto>();
            foreach (var item in items)
            {
                dtoList.Add(await MapToItemMatchDto(item));
            }

            return new PagedResponse<ItemMatchDto>(dtoList, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<PagedResponse<ItemMatchDto>> GetMyMatchesPagingAsync(int userId, PagingParameters pagingParameters)
        {
            var (items, totalCount) = await _matchingRepository.GetMatchesPagingAsync(userId, pagingParameters.PageNumber, pagingParameters.PageSize);

            var dtoList = new List<ItemMatchDto>();
            foreach (var item in items)
            {
                dtoList.Add(await MapToItemMatchDto(item));
            }

            return new PagedResponse<ItemMatchDto>(dtoList, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<IEnumerable<ItemMatchDto>> GetApprovedMatchesAsync()
        {
            var matches = await _matchingRepository.GetAllByStatusAsync("Approved");
            var dtoList = new List<ItemMatchDto>();
            foreach (var match in matches)
            {
                dtoList.Add(await MapToItemMatchDto(match));
            }
            return dtoList;
        }

        private FoundItemDto MapToFoundItemDto(FoundItem foundItem)
        {
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
                ImageUrls = foundItem.Images.Select(i => i.ImageUrl).ToList(),
                // ClaimRequests and ActionLogs are not mapped here to keep it simple, as they are often fetched separately or not needed for a summary.
            };
        }

        private LostItemDto MapToLostItemDto(LostItem lostItem)
        {
            return new LostItemDto
            {
                LostItemId = lostItem.LostItemId,
                Title = lostItem.Title,
                Description = lostItem.Description,
                LostDate = lostItem.LostDate,
                LostLocation = lostItem.LostLocation,
                Status = lostItem.Status,
                CampusId = lostItem.CampusId,
                CampusName = lostItem.Campus?.CampusName,
                CategoryId = lostItem.CategoryId,
                CategoryName = lostItem.Category?.CategoryName,
                ImageUrls = lostItem.Images.Select(i => i.ImageUrl).ToList(),
                // ActionLogs not mapped here to keep it simple.
            };
        }
    }
}