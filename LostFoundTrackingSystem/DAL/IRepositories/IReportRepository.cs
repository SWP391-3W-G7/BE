using DAL.Models;

namespace DAL.IRepositories
{
    public interface IReportRepository
    {
        Task<DashboardStatisticsModel> GetSystemStatsAsync(int? campusId);
    }
}