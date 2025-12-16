using System.Collections.Generic;

namespace DAL.Models
{
    public class DashboardStatisticsModel
    {
        public int TotalFound { get; set; }
        public int ReturnedCount { get; set; }
        public int DisposedCount { get; set; }
        public int ActiveClaims { get; set; }
        public Dictionary<string, int> CategoryStats { get; set; }
    }
}
