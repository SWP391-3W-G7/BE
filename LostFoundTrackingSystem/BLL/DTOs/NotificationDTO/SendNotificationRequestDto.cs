using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.NotificationDTO
{
    public class SendNotificationRequestDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
