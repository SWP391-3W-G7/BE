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

        public async Task<SystemReportDto> GetDashboardReportAsync(string roleName, int? campusId)
        {
            DashboardStatisticsModel rawData;

            if (roleName == "Admin")
            {
                rawData = await _repo.GetSystemStatsAsync(null);
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

            double returnRate = 0;
            if (rawData.TotalFound > 0)
            {
                returnRate = Math.Round((double)rawData.ReturnedCount / rawData.TotalFound * 100, 2);
            }

            return new SystemReportDto
            {
                TotalFound = rawData.TotalFound,
                ReturnedCount = rawData.ReturnedCount,
                DisposedCount = rawData.DisposedCount,
                ActiveClaims = rawData.ActiveClaims,
                ReturnRate = returnRate,
                CategoryStats = rawData.CategoryStats.Select(cs => new CategoryStatDto { Name = cs.Key, Value = cs.Value }).ToList()
            };
        }
    }
}