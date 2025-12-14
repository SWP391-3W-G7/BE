using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.FoundItemDTO
{
    public class UpdateFoundItemStatusRequest
    {
        [Required]
        public string Status { get; set; } = null!;
    }
}
