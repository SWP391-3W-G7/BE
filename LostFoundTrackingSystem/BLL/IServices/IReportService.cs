using BLL.DTOs.ReportDTO;

namespace BLL.IServices
{
    public interface IReportService
    {
        // Thay string roleName bằng int roleId
        Task<SystemReportDto> GetDashboardReportAsync(int roleId, int? campusId);
    }
}