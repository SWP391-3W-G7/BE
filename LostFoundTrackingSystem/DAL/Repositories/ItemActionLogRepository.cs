using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ItemActionLogRepository : IItemActionLogRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public ItemActionLogRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ItemActionLog log)
        {
            await _context.ItemActionLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ItemActionLog>> GetByFoundItemIdAsync(int foundItemId)
        {
            return await _context.ItemActionLogs
                .Include(l => l.PerformedByNavigation)
                .Include(l => l.Campus)
                .Where(l => l.FoundItemId == foundItemId)
                .ToListAsync();
        }

        public async Task<List<ItemActionLog>> GetByLostItemIdAsync(int lostItemId)
        {
            return await _context.ItemActionLogs
                .Include(l => l.PerformedByNavigation)
                .Include(l => l.Campus)
                .Where(l => l.LostItemId == lostItemId)
                .ToListAsync();
        }

        public async Task<List<ItemActionLog>> GetByClaimRequestIdAsync(int claimRequestId)
        {
            return await _context.ItemActionLogs
                .Include(l => l.PerformedByNavigation)
                .Include(l => l.Campus)
                .Where(l => l.ClaimRequestId == claimRequestId)
                .ToListAsync();
        }
    }
}
