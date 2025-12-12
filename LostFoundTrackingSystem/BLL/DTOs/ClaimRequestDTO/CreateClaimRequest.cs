using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.ClaimRequestDTO
{
    public class CreateClaimRequest
    {
        [Required]
        public int FoundItemId { get; set; }
        [Required]
        public string EvidenceTitle { get; set; } = null!;
        [Required]
        public string EvidenceDescription { get; set; } = null!;
        public List<IFormFile>? EvidenceImages { get; set; }
        [Required]
        public int CampusId { get; set; }
    }
}