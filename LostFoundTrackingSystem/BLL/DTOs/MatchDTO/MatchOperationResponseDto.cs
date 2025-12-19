using BLL.DTOs.FoundItemDTO;
using BLL.DTOs.LostItemDTO;

namespace BLL.DTOs.MatchDTO
{
    public class MatchOperationResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? MatchId { get; set; } // Optional: to include the ID of the match that was acted upon
        public LostItemDto? LostItem { get; set; }
        public FoundItemDto? FoundItem { get; set; }
    }
}
