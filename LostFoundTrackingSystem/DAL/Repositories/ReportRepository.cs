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

        public async Task<SystemStatisticModel> GetSystemStatsAsync(int? campusId)
        {
            var lostQuery = _context.LostItems.AsQueryable();
            var foundQuery = _context.FoundItems.AsQueryable();

            if (campusId.HasValue)
            {
                lostQuery = lostQuery.Where(x => x.CampusId == campusId);
                foundQuery = foundQuery.Where(x => x.CampusId == campusId);
            }

            var stats = new SystemStatisticModel
            {
                TotalLost = await lostQuery.CountAsync(),
                TotalFound = await foundQuery.CountAsync(),
                InStorage = await foundQuery.CountAsync(x => x.Status == "Stored")
            };

            return stats;
        }
    }
}