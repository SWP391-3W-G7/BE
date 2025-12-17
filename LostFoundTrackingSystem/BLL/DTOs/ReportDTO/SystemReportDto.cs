namespace BLL.DTOs.ReportDTO
{
    public class SystemReportDto
    {
        public int TotalFound { get; set; }
        public int TotalClaims { get; set; }

        public List<StatItemDto> FoundItemStats { get; set; }
        public List<StatItemDto> ClaimStats { get; set; }
        public List<CategoryStatDto> CategoryStats { get; set; }
    }
    public class StatItemDto
    {
        public string StatusName { get; set; }
        public int Count { get; set; }
    }
}