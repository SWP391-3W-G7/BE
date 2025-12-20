using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.LostItemDTO
{
    public class LostItemStatisticDto
    {
        public int TotalLost { get; set; }
        public int TotalMatched { get; set; }
        public int TotalReturned { get; set; }
        public int TotalItems { get; set; }
    }
}
