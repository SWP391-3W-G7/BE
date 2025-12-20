using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.LostItemDTO
{
    public class TopCampusStatDto
    {
        public int CampusId { get; set; }
        public string CampusName { get; set; } = null!;
        public int TotalLostItems { get; set; }
    }
}
