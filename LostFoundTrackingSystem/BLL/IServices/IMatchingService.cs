using BLL.DTOs.MatchDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IMatchingService
    {
        Task FindAndCreateMatchesAsync(int lostItemId);
        Task<IEnumerable<ItemMatchDto>> GetMatchesForFoundItemAsync(int foundItemId);
        Task<IEnumerable<ItemMatchDto>> GetMatchesForLostItemAsync(int lostItemId);
        Task FindAndCreateMatchesForAllLostItemsAsync();
        Task ConfirmMatchAsync(int matchId, int staffUserId);
        Task DismissMatchAsync(int matchId, int staffUserId);
    }
}
