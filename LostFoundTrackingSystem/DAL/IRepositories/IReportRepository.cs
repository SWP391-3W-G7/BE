using DAL.Models;

namespace DAL.IRepositories
{
    public interface IReportRepository
    {
        Task<SystemStatisticModel> GetSystemStatsAsync(int? campusId);
    }
}