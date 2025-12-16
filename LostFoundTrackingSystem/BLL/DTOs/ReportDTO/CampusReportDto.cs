namespace BLL.DTOs.ReportDTO
{
    public class CampusReportDto : SystemReportDto
    {
        public int CampusId { get; set; }
        public string CampusName { get; set; }
    }
}