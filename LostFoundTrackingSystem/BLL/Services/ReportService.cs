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
            SystemStatisticModel rawData;

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

            return new SystemReportDto
            {
                TotalLostItems = rawData.TotalLost,
                TotalFoundItems = rawData.TotalFound,
                ItemsInStorage = rawData.InStorage
            };
        }
    }
}