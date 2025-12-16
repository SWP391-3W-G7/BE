using BLL.DTOs.NotificationDTO;
using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequestDto request)
        {
            await _notificationService.SendGenericNotificationAsync(request.UserId, request.Message);
            return Ok(new { message = "Notification sent successfully." });
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            return Ok(notifications);
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);
            return Ok(new { count = notifications.Count });
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }
    }
}