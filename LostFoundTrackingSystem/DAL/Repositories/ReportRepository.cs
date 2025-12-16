using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public ReportRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatisticsModel> GetSystemStatsAsync(int? campusId)
        {
            var foundQuery = _context.FoundItems.AsQueryable();
            var claimQuery = _context.ClaimRequests.AsQueryable();

            // 1. Filter theo Campus nếu có
            if (campusId.HasValue)
            {
                foundQuery = foundQuery.Where(x => x.CampusId == campusId.Value);
                claimQuery = claimQuery.Where(x => x.FoundItem.CampusId == campusId.Value);
            }

            var foundStatsRaw = await foundQuery
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var claimStatsRaw = await claimQuery
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var categoryStats = await foundQuery
                .Include(x => x.Category)
                .Where(x => x.Category != null)
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new { Name = g.Key, Value = g.Count() })
                .ToDictionaryAsync(k => k.Name, v => v.Value);

            var model = new DashboardStatisticsModel
            {
                TotalFoundItems = await foundQuery.CountAsync(),
                TotalClaimRequests = await claimQuery.CountAsync(),
                CategoryStats = categoryStats,
                FoundItemStatusStats = new Dictionary<string, int>(),
                ClaimStatusStats = new Dictionary<string, int>()
            };

            foreach (FoundItemStatus status in Enum.GetValues(typeof(FoundItemStatus)))
            {
                string statusName = status.ToString();
                model.FoundItemStatusStats[statusName] = foundStatsRaw.ContainsKey(statusName) ? foundStatsRaw[statusName] : 0;
            }

            foreach (ClaimStatus status in Enum.GetValues(typeof(ClaimStatus)))
            {
                string statusName = status.ToString();
                model.ClaimStatusStats[statusName] = claimStatsRaw.ContainsKey(statusName) ? claimStatsRaw[statusName] : 0;
            }

            return model;
        }
    }
}