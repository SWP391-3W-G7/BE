using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

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

            if (campusId.HasValue)
            {
                foundQuery = foundQuery.Where(x => x.CampusId == campusId.Value);
                // This join is not ideal, but for the sake of getting it done
                claimQuery = claimQuery.Where(x => x.FoundItem.CampusId == campusId.Value);
            }

            var totalFound = await foundQuery.CountAsync();
            var returnedCount = await foundQuery.CountAsync(x => x.Status == "Returned");
            var disposedCount = await foundQuery.CountAsync(x => x.Status == "Disposed");
            var activeClaims = await claimQuery.CountAsync(x => x.Status == "Pending" || x.Status == "InProgress");

            var categoryStats = await foundQuery
                .Include(x => x.Category)
                .Where(x => x.Category != null)
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new { Name = g.Key, Value = g.Count() })
                .ToDictionaryAsync(k => k.Name, v => v.Value);

            var stats = new DashboardStatisticsModel
            {
                TotalFound = totalFound,
                ReturnedCount = returnedCount,
                DisposedCount = disposedCount,
                ActiveClaims = activeClaims,
                CategoryStats = categoryStats
            };

            return stats;
        }
    }
}