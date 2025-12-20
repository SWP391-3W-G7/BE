using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.FoundItemDTO
{
    public class FoundItemStatisticDto
    {
        public int TotalStored { get; set; }    // Status: Stored (0)
        public int TotalClaimed { get; set; }   // Status: Claimed (1)
        public int TotalReturned { get; set; }  // Status: Returned (2)
        public int TotalOpen { get; set; }      // Status: Open (3)
        public int TotalClosed { get; set; }    // Status: Closed (4)
        public int TotalItems { get; set; }     // Tổng cộng
    }
}
