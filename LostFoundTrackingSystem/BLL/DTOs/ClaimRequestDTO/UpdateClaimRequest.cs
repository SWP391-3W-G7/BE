using DAL.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.ClaimRequestDTO
{
    public class UpdateClaimRequest
    {
        [Required]
        public string EvidenceTitle { get; set; } = null!;

        [Required]
        public string EvidenceDescription { get; set; } = null!;
        public ClaimPriority Priority { get; set; }
        public List<IFormFile>? NewImages { get; set; }
    }
}