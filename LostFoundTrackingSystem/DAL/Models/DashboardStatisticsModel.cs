using System.Collections.Generic;

namespace DAL.Models
{
    public class DashboardStatisticsModel
    {
        public int TotalFoundItems { get; set; }
        public int TotalClaimRequests { get; set; }
        public Dictionary<string, int> FoundItemStatusStats { get; set; }
        public Dictionary<string, int> ClaimStatusStats { get; set; }
        public Dictionary<string, int> CategoryStats { get; set; }
    }
}
