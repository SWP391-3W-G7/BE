using DAL.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LostFoundApi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub 
    {
        private static readonly Dictionary<string, string> UserConnections = new();
        private readonly INotificationRepository _notificationRepo;

        public NotificationHub(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;
                Console.WriteLine($"User {userId} connected with ConnectionId: {Context.ConnectionId}");

                await SendUnsentNotifications(userId);
            }

            await base.OnConnectedAsync();
        }

        private async Task SendUnsentNotifications(string userId)
        {
            try
            {
                var unsentNotifications = await _notificationRepo.GetByUserIdAsync(
                    int.Parse(userId),
                    unreadOnly: false
                );

                var unsent = unsentNotifications.Where(n => !n.IsSent).ToList();

                foreach (var notification in unsent)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveNotification", new
                    {
                        notificationId = notification.NotificationId,
                        type = notification.Type,
                        referenceId = notification.ReferenceId,
                        message = notification.Message,
                        timestamp = notification.CreatedAt
                    });

                    await _notificationRepo.MarkAsSentAsync(notification.NotificationId);
                }

                if (unsent.Any())
                {
                    Console.WriteLine($"Sent {unsent.Count} unsent notifications to user {userId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending unsent notifications: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId) && UserConnections.ContainsKey(userId))
            {
                UserConnections.Remove(userId);
                Console.WriteLine($"User {userId} disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }
        public static string? GetConnectionId(string userId)
        {
            return UserConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }
    }
}
