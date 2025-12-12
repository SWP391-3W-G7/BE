using DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IMatchingService
    {
        Task FindAndCreateMatchesAsync(int lostItemId);
        Task<IEnumerable<ItemMatch>> GetMatchesForFoundItemAsync(int foundItemId);
        Task FindAndCreateMatchesForAllLostItemsAsync();
        Task ConfirmMatchAsync(int matchId, int staffUserId);
        Task DismissMatchAsync(int matchId, int staffUserId);
    }
}
