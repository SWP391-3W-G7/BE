using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.MatchDTO;
using BLL.DTOs.StaffDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.DTOs.LostItemDTO;
using BLL.DTOs.FoundItemDTO;
using BLL.DTOs;
using BLL.DTOs.Paging;

namespace BLL.Services
{
    public class StaffService : IStaffService
    {
        private readonly IClaimRequestRepository _claimRequestRepository;
        private readonly IMatchingRepository _matchingRepository;
        private readonly IItemActionLogService _itemActionLogService;
        private readonly IClaimRequestService _claimRequestService;
        private readonly IFoundItemRepository _foundItemRepo;
        private readonly INotificationService _notifService;
        private readonly IUserRepository _userRepo;

        public StaffService(IClaimRequestRepository claimRequestRepository, IMatchingRepository matchingRepository, IItemActionLogService itemActionLogService, IClaimRequestService claimRequestService, IFoundItemRepository foundItemRepo, INotificationService notifService, IUserRepository userRepo)
        {
            _claimRequestRepository = claimRequestRepository;
            _matchingRepository = matchingRepository;
            _itemActionLogService = itemActionLogService;
            _claimRequestService = claimRequestService;
            _foundItemRepo = foundItemRepo;
            _notifService = notifService;
            _userRepo = userRepo;
        }

        public async Task<StaffWorkItemsDto> GetWorkItemsAsync(int campusId, PagingParameters pagingParameters)
        {
            // Claims
            var pendingClaims = await _claimRequestRepository.GetAllAsync(ClaimStatus.Pending);
            var conflictedClaims = await _claimRequestRepository.GetAllAsync(ClaimStatus.Conflicted);
            var allClaims = pendingClaims.Concat(conflictedClaims)
                                         .Where(c => c.FoundItem?.CampusId == campusId)
                                         .ToList();
            var totalClaimsCount = allClaims.Count();
            var pagedClaims = allClaims
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToList();

            var claimDtos = new List<ClaimRequestDto>();
            foreach (var claim in pagedClaims)
            {
                claimDtos.Add(await MapToClaimRequestDto(claim));
            }
            var pagedClaimsResponse = new PagedResponse<ClaimRequestDto>(claimDtos, totalClaimsCount, pagingParameters.PageNumber, pagingParameters.PageSize);

            // Matches
            var matchedItems = (await _matchingRepository.GetAllByStatusAsync("Matched"))
                                         .Where(m => m.FoundItem?.CampusId == campusId || m.LostItem?.CampusId == campusId)
                                         .ToList();
            var totalMatchesCount = matchedItems.Count();
            var pagedMatches = matchedItems
                .Skip((pagingParameters.PageNumber - 1) * pagingParameters.PageSize)
                .Take(pagingParameters.PageSize)
                .ToList();
            
            var matchDtos = new List<ItemMatchDto>();
            foreach (var match in pagedMatches)
            {
                matchDtos.Add(await MapToItemMatchDto(match));
            }
            var pagedMatchesResponse = new PagedResponse<ItemMatchDto>(matchDtos, totalMatchesCount, pagingParameters.PageNumber, pagingParameters.PageSize);


            var workItems = new StaffWorkItemsDto
            {
                PendingAndConflictedClaims = pagedClaimsResponse,
                MatchedItems = pagedMatchesResponse
            };

            return workItems;
        }

        private async Task<ClaimRequestDto> MapToClaimRequestDto(ClaimRequest c)
        {
            var actionLogs = await _itemActionLogService.GetLogsByClaimRequestIdAsync(c.ClaimId);

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
                }).ToList(),
                ActionLogs = actionLogs
            };
        }

        private async Task<ItemMatchDto> MapToItemMatchDto(ItemMatch match)
        {
            var foundItemClaims = new List<ClaimRequestDto>();
            if (match.FoundItem?.FoundItemId != null)
            {
                var claims = await _claimRequestRepository.GetByFoundItemIdAsync(match.FoundItem.FoundItemId);
                foundItemClaims = claims.Select(c => MapToClaimRequestDto(c).Result).ToList();
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
        public async Task RequestItemDropOffAsync(int foundItemId, RequestDropOffDto request, int staffId)
        {
            var item = await _foundItemRepo.GetByIdAsync(foundItemId);
            if (item == null) throw new Exception("Found item not found.");

            if (item.Status != "Open")
                throw new Exception($"Cannot request drop-off for item with status '{item.Status}'. Item must be 'Open'.");

            if (item.CreatedBy == null)
                throw new Exception("This item has no associated finder (CreatedBy is null).");

            string studentId = item.CreatedBy.Value.ToString();
            string message = $"ACTION REQUIRED: Please bring the found item '{item.Title}' to {request.StorageLocation}. " +
                             $"{(string.IsNullOrEmpty(request.Note) ? "" : $"Note: {request.Note}")}";

            try
            {
                await _notifService.SendNotificationAsync(studentId, item.FoundItemId, "DropOffRequested", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification failed: {ex.Message}");
            }

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                FoundItemId = item.FoundItemId,
                ActionType = "DropOffRequested",
                ActionDetails = $"Staff requested student to bring item to: {request.StorageLocation}. Note: {request.Note ?? "None"}",
                OldStatus = item.Status,
                NewStatus = item.Status, 
                PerformedBy = staffId,
                CampusId = item.CampusId
            });
        }
    }
}