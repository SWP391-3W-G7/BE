using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.AdminDTO
{
    public class CreateCampusRequest
    {
        [Required]
        public string CampusName { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        public string? StorageLocation { get; set; }
    }
}