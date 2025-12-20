namespace BLL.DTOs.LostItemDTO
{
    public class TopUserLostItemDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public int TotalLostItems { get; set; } 
    }
}