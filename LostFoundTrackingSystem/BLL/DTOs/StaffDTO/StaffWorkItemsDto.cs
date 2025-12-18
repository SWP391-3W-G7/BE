using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.MatchDTO;
using BLL.DTOs.Paging;

namespace BLL.DTOs.StaffDTO
{
    public class StaffWorkItemsDto
    {
        public PagedResponse<ClaimRequestDto> PendingAndConflictedClaims { get; set; }
        public PagedResponse<ItemMatchDto> MatchedItems { get; set; }
    }
}
