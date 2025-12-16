using BLL.DTOs.NotificationDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using Microsoft.AspNetCore.SignalR;

namespace LostFoundApi.Hubs
{
    public class SignalRNotification : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepo;
        public SignalRNotification(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepo)
        {
            _hubContext = hubContext;
            _notificationRepo = notificationRepo;
        }
        public async Task SendNotificationAsync(string userId, int claimId, string status, string message)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = int.Parse(userId),
                    Type = "claim",
                    ReferenceId = claimId,
                    Message = message,
                    IsRead = false,
                    IsSent = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepo.AddAsync(notification);
                var connectionId = NotificationHub.GetConnectionId(userId);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", new
                    {
                        notificationId = notification.NotificationId,
                        type = "claim",
                        claimId,
                        status,
                        message,
                        timestamp = DateTime.UtcNow
                    });

                    await _notificationRepo.MarkAsSentAsync(notification.NotificationId);
                    Console.WriteLine($"Notification sent to user {userId}: {message}");
                }
                else
                {
                    Console.WriteLine($"User {userId} is not connected. Notification saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }
        public async Task SendMatchNotificationAsync(string userId, int matchId, string message)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = int.Parse(userId),
                    Type = "match",
                    ReferenceId = matchId,
                    Message = message,
                    IsRead = false,
                    IsSent = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepo.AddAsync(notification);

                var connectionId = NotificationHub.GetConnectionId(userId);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", new
                    {
                        notificationId = notification.NotificationId,
                        type = "match",
                        matchId,
                        message,
                        timestamp = DateTime.UtcNow
                    });

                    await _notificationRepo.MarkAsSentAsync(notification.NotificationId);
                    Console.WriteLine($"Match notification sent to user {userId}: {message}");
                }
                else
                {
                    Console.WriteLine($"User {userId} is not connected. Notification saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending match notification: {ex.Message}");
            }
        }
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
        {
            var notifications = await _notificationRepo.GetByUserIdAsync(userId, unreadOnly);
            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                ReferenceId = n.ReferenceId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            }).ToList();
        }
        public async Task MarkAsReadAsync(int notificationId)
        {
            await _notificationRepo.MarkAsReadAsync(notificationId);
        }

        public async Task SendGenericNotificationAsync(string userId, string message)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = int.Parse(userId),
                    Type = "generic",
                    Message = message,
                    IsRead = false,
                    IsSent = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepo.AddAsync(notification);
                var connectionId = NotificationHub.GetConnectionId(userId);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", new
                    {
                        notificationId = notification.NotificationId,
                        type = "generic",
                        message,
                        timestamp = DateTime.UtcNow
                    });

                    await _notificationRepo.MarkAsSentAsync(notification.NotificationId);
                    Console.WriteLine($"Generic notification sent to user {userId}: {message}");
                }
                else
                {
                    Console.WriteLine($"User {userId} is not connected. Notification saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending generic notification: {ex.Message}");
            }
        }
    }
}
