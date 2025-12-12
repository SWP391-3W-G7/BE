using BLL.DTOs.ReportDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;

namespace BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        // Định nghĩa hằng số cho dễ đọc code
        private const int ROLE_ADMIN = 4;
        private const int ROLE_STAFF = 2;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<SystemReportDto> GetDashboardReportAsync(int roleId, int? campusId)
        {
            SystemStatisticModel rawData;

            if (roleId == ROLE_ADMIN) 
            {
                rawData = await _repo.GetSystemStatsAsync(null);
            }
            else if (roleId == ROLE_STAFF)
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