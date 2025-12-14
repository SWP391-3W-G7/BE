using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.LostItemDTO
{
    public class UpdateLostItemStatusRequest
    {
        [Required]
        public string Status { get; set; } = null!;
    }
}
