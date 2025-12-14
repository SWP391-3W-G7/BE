using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.ClaimRequestDTO
{
    public class AddEvidenceRequest
    {
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
