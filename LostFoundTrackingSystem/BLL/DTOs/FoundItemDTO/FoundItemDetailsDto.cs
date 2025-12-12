using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.LostItemDTO;
using System.Collections.Generic;

namespace BLL.DTOs.FoundItemDTO
{
    public class FoundItemDetailsDto : FoundItemDto
    {
        public List<LostItemDto> ApprovedLostItems { get; set; }
        public List<ClaimRequestDto> ApprovedClaimRequests { get; set; }
    }
}
