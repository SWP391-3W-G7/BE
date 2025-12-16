using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.CampusDTO
{
    public class CreateCampusDto
    {
        [Required]
        public string CampusName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string StorageLocation { get; set; }
    }
}