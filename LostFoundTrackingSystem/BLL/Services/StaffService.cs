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

namespace BLL.Services
{
    public class StaffService : IStaffService
    {
        private readonly IClaimRequestRepository _claimRequestRepository;
        private readonly IMatchingRepository _matchingRepository;
        private readonly IItemActionLogService _itemActionLogService;
        private readonly IClaimRequestService _claimRequestService;

        public StaffService(IClaimRequestRepository claimRequestRepository, IMatchingRepository matchingRepository, IItemActionLogService itemActionLogService, IClaimRequestService claimRequestService)
        {
            _claimRequestRepository = claimRequestRepository;
            _matchingRepository = matchingRepository;
            _itemActionLogService = itemActionLogService;
            _claimRequestService = claimRequestService;
        }

        public async Task<StaffWorkItemsDto> GetWorkItemsAsync(int campusId)
        {
            var pendingClaims = await _claimRequestRepository.GetAllAsync(ClaimStatus.Pending);
            var conflictedClaims = await _claimRequestRepository.GetAllAsync(ClaimStatus.Conflicted);
            var allClaims = pendingClaims.Concat(conflictedClaims)
                                         .Where(c => c.FoundItem?.CampusId == campusId)
                                         .ToList();

            var matchedItems = (await _matchingRepository.GetAllByStatusAsync("Matched"))
                                         .Where(m => m.FoundItem?.CampusId == campusId || m.LostItem?.CampusId == campusId)
                                         .ToList();

            var claimDtos = new List<ClaimRequestDto>();
            foreach (var claim in allClaims)
            {
                claimDtos.Add(await MapToClaimRequestDto(claim));
            }

            var matchDtos = new List<ItemMatchDto>();
            foreach (var match in matchedItems)
            {
                matchDtos.Add(await MapToItemMatchDto(match));
            }

            var workItems = new StaffWorkItemsDto
            {
                PendingAndConflictedClaims = claimDtos,
                MatchedItems = matchDtos
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
    }
}