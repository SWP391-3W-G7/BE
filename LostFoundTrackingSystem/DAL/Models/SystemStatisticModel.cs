using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class SystemStatisticModel
    {
        public int TotalLost { get; set; }
        public int TotalFound { get; set; }
        public int InStorage { get; set; }
    }
}
