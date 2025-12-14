using DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.IRepositories
{
    public interface IItemActionLogRepository
    {
        Task AddAsync(ItemActionLog log);
        Task<List<ItemActionLog>> GetByFoundItemIdAsync(int foundItemId);
        Task<List<ItemActionLog>> GetByLostItemIdAsync(int lostItemId);
        Task<List<ItemActionLog>> GetByClaimRequestIdAsync(int claimRequestId);
    }
}
