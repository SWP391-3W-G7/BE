namespace BLL.DTOs.NotificationDTO
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Type { get; set; }
        public int? ReferenceId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}