using DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.IRepositories
{
    public interface IMatchingRepository
    {
        Task<IEnumerable<ItemMatch>> GetMatchesForLostItemAsync(int lostItemId);
        Task<IEnumerable<ItemMatch>> GetMatchesForFoundItemAsync(int foundItemId);
        Task AddMatchAsync(ItemMatch itemMatch);
        Task<ItemMatch?> GetExistingMatchAsync(int lostItemId, int foundItemId);
        Task<IEnumerable<FoundItem>> GetPotentialMatchesAsync(LostItem lostItem);
        Task<ItemMatch> GetMatchByIdAsync(int matchId);
        Task UpdateMatchAsync(ItemMatch itemMatch);
    }
}
