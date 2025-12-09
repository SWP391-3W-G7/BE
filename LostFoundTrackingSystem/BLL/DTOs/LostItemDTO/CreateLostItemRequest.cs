using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.LostItemDTO
{
    public class CreateLostItemRequest
    {
        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime LostDate { get; set; }

        [Required]
        public string LostLocation { get; set; } = null!;

        [Required]
        public int CampusId { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int CreatedBy { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
