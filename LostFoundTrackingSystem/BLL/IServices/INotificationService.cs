using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.NotificationDTO;

namespace BLL.IServices
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, int claimId, string status, string message);
        Task SendMatchNotificationAsync(string userId, int matchId, string message);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task MarkAsReadAsync(int notificationId);
    }
}
