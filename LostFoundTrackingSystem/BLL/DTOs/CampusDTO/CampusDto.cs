namespace BLL.DTOs.CampusDTO
{
    public class CampusDto
    {
        public int CampusId { get; set; }
        public required string CampusName { get; set; }
        public string? Address { get; set; }
        public string? StorageLocation { get; set; }
    }
}
