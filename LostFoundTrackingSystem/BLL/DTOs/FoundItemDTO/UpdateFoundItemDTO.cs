using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BLL.DTOs.FoundItemDTO
{
    public class UpdateFoundItemDTO
    {
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? LocationFound { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
