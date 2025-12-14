using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.LostItemDTO;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class UserActivityDto
    {
        public List<ClaimRequestDto> ClaimRequests { get; set; } = new();
        public List<LostItemDto> LostItems { get; set; } = new();
    }
}
