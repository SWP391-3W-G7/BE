using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.MatchDTO;
using System.Collections.Generic;

namespace BLL.DTOs.StaffDTO
{
    public class StaffWorkItemsDto
    {
        public List<ClaimRequestDto> PendingAndConflictedClaims { get; set; }
        public List<ItemMatchDto> MatchedItems { get; set; }
    }
}
