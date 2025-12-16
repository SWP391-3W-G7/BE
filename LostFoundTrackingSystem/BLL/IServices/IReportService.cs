using BLL.DTOs.ReportDTO;

namespace BLL.IServices
{
    public interface IReportService
    {
        Task<SystemReportDto> GetDashboardReportAsync(string roleName, int? campusId);
    }
}