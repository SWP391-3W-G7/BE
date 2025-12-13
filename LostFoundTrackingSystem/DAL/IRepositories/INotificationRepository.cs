using DAL.Models;

namespace DAL.IRepositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false);
        Task<Notification> GetByIdAsync(int id);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAsSentAsync(int notificationId);
        Task<List<Notification>> GetUnsentNotificationsAsync();
    }
}