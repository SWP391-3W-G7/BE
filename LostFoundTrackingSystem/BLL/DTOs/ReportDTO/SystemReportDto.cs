namespace BLL.DTOs.ReportDTO
{
    public class SystemReportDto
    {
        public int TotalLostItems { get; set; }
        public int TotalFoundItems { get; set; }
        public int ItemsInStorage { get; set; } // Items with status "Stored"
    }
}