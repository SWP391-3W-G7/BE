using BLL.DTOs; // Added to access ItemActionLogDto
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.LostItemDTO
{
    public class LostItemDto
    {
        public int LostItemId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? LostDate { get; set; }
        public string? LostLocation { get; set; }
        public string? Status { get; set; }
        public int? CampusId { get; set; }
        public string? CampusName { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public List<string> ImageUrls { get; set; } = new();
        public List<ItemActionLogDto>? ActionLogs { get; set; } // Added
    }
}

