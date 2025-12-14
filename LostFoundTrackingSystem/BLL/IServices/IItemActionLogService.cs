using BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IItemActionLogService
    {
        Task AddLogAsync(ItemActionLogDto logDto);
        Task<List<ItemActionLogDto>> GetLogsByFoundItemIdAsync(int foundItemId);
        Task<List<ItemActionLogDto>> GetLogsByLostItemIdAsync(int lostItemId);
        Task<List<ItemActionLogDto>> GetLogsByClaimRequestIdAsync(int claimRequestId);
    }
}
