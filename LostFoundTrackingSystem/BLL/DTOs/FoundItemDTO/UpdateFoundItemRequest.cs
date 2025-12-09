using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.FoundItemDTO
{
    public class UpdateFoundItemRequest
    {
        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public DateTime FoundDate { get; set; }

        [Required]
        public string FoundLocation { get; set; } = null!;

        [Required]
        public int CampusId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? StoredBy { get; set; }

        public List<IFormFile>? NewImages { get; set; }
    }
}