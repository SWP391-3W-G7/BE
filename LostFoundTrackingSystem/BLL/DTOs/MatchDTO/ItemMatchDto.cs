using BLL.DTOs;
using BLL.DTOs.FoundItemDTO;
using BLL.DTOs.LostItemDTO;
using System;
using System.Collections.Generic;

namespace BLL.DTOs.MatchDTO
{
    public class ItemMatchDto
    {
        public int MatchId { get; set; }
        public string? MatchStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; } // e.g., Pending, Approved, Dismissed

        public int? LostItemId { get; set; }
        public LostItemDto? LostItem { get; set; } // Include LostItemDto

        public int? FoundItemId { get; set; }
        public FoundItemDto? FoundItem { get; set; } // Include FoundItemDto

        public int? CreatedBy { get; set; }
        public UserDto? CreatedByNavigation { get; set; } // Include UserDto
    }
}
