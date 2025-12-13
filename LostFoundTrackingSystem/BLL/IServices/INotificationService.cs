using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, int claimId, string status, string message);
        Task SendMatchNotificationAsync(string userId, int matchId, string message);
    }
}
