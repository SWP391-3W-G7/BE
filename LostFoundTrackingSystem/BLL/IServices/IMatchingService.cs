using BLL.DTOs.MatchDTO;
using BLL.DTOs.Paging;
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
        Task ConflictMatchAsync(int matchId, int staffUserId);
        Task<ItemMatchDto> GetMatchDetailsByIdAsync(int matchId);
        Task<PagedResponse<ItemMatchDto>> GetAllMatchesPagingAsync(PagingParameters pagingParameters);
        Task<PagedResponse<ItemMatchDto>> GetMyMatchesPagingAsync(int userId, PagingParameters pagingParameters);
    }
}
