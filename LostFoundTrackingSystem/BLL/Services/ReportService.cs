using BLL.DTOs.ReportDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;

namespace BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<SystemReportDto> GetDashboardReportAsync(string roleName, int? campusId, int? filterCampusId)
        {
            DashboardStatisticsModel rawData;

            if (roleName == "Admin")
            {
                rawData = await _repo.GetSystemStatsAsync(filterCampusId);
            }
            else if (roleName == "Staff")
            {
                if (campusId == null)
                {
                    throw new Exception("Staff user does not have a valid Campus assigned.");
                }
                rawData = await _repo.GetSystemStatsAsync(campusId);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not authorized to view this report.");
            }

            int returnedCount = 0;

            if (rawData.FoundItemStatusStats.ContainsKey(FoundItemStatus.Returned.ToString()))
            {
                returnedCount = rawData.FoundItemStatusStats[FoundItemStatus.Returned.ToString()];
            }

            double returnRate = 0;
            if (rawData.TotalFoundItems > 0)
            {
                returnRate = Math.Round((double)returnedCount / rawData.TotalFoundItems * 100, 2);
            }

            return new SystemReportDto
            {
                TotalFound = rawData.TotalFoundItems,
                TotalClaims = rawData.TotalClaimRequests,

                FoundItemStats = rawData.FoundItemStatusStats.Select(x => new StatItemDto
                {
                    StatusName = x.Key,
                    Count = x.Value
                }).ToList(),

                ClaimStats = rawData.ClaimStatusStats.Select(x => new StatItemDto
                {
                    StatusName = x.Key,
                    Count = x.Value
                }).ToList(),

                CategoryStats = rawData.CategoryStats.Select(cs => new CategoryStatDto
                {
                    Name = cs.Key,
                    Value = cs.Value
                }).ToList()
            };
        }
    }
}