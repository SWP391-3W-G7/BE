namespace BLL.DTOs.Security
{
    public class SecurityFoundItemDto
    {
        public int FoundItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime FoundDate { get; set; }
        public string FoundLocation { get; set; }
        public string Status { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<string> ImageUrls { get; set; }
    }
}
