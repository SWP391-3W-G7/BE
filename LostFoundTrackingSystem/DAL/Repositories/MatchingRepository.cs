using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class MatchingRepository : IMatchingRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public MatchingRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task AddMatchAsync(ItemMatch itemMatch)
        {
            await _context.ItemMatches.AddAsync(itemMatch);
            await _context.SaveChangesAsync();
        }

        public async Task<ItemMatch?> GetExistingMatchAsync(int lostItemId, int foundItemId)
        {
            return await _context.ItemMatches
                .Where(m => m.LostItemId == lostItemId && m.FoundItemId == foundItemId && m.Status == "Pending")
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ItemMatch>> GetMatchesForFoundItemAsync(int foundItemId)
        {
            return await _context.ItemMatches
                .Include(m => m.FoundItem)
                .Include(m => m.LostItem)
                .Include(m => m.CreatedByNavigation)
                .Where(m => m.FoundItemId == foundItemId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ItemMatch>> GetMatchesForLostItemAsync(int lostItemId)
        {
            return await _context.ItemMatches
                .Include(m => m.FoundItem)
                .Include(m => m.LostItem)
                .Include(m => m.CreatedByNavigation)
                .Where(m => m.LostItemId == lostItemId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FoundItem>> GetPotentialMatchesAsync(LostItem lostItem)
        {
            var query = _context.FoundItems
                .Include(f => f.Images) // Include images for more detailed matching later if needed
                .Where(f => f.CategoryId == lostItem.CategoryId &&
                             f.CampusId == lostItem.CampusId &&
                             (f.Status == FoundItemStatus.Stored.ToString() || f.Status == FoundItemStatus.Open.ToString()));

            // Simple keyword matching for title and description
            if (!string.IsNullOrWhiteSpace(lostItem.Title))
            {
                query = query.Where(f => f.Title.Contains(lostItem.Title) || f.Description.Contains(lostItem.Title));
            }

            if (!string.IsNullOrWhiteSpace(lostItem.Description))
            {
                query = query.Where(f => f.Description.Contains(lostItem.Description) || f.Title.Contains(lostItem.Description));
            }

            return await query.ToListAsync();
        }

        public async Task<ItemMatch> GetMatchByIdAsync(int matchId)
        {
            return await _context.ItemMatches
                .Include(m => m.LostItem)
                .Include(m => m.FoundItem)
                .Include(m => m.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);
        }

        public async Task UpdateMatchAsync(ItemMatch itemMatch)
        {
            _context.ItemMatches.Update(itemMatch);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ItemMatch>> GetAllByStatusAsync(string status)
        {
            return await _context.ItemMatches
                .Include(m => m.FoundItem)
                .Include(m => m.LostItem)
                .Include(m => m.CreatedByNavigation)
                .Where(m => m.MatchStatus == status)
                .ToListAsync();
        }
        public async Task<(IEnumerable<ItemMatch> Items, int TotalCount)> GetMatchesPagingAsync(int? userId, int pageNumber, int pageSize)
        {
            var query = _context.ItemMatches
                .Include(m => m.CreatedByNavigation).ThenInclude(u => u.Role)
                .Include(m => m.CreatedByNavigation).ThenInclude(u => u.Campus)
                .Include(m => m.FoundItem).ThenInclude(f => f.Images)
                .Include(m => m.FoundItem).ThenInclude(f => f.Campus)
                .Include(m => m.FoundItem).ThenInclude(f => f.Category)
                .Include(m => m.LostItem).ThenInclude(l => l.Images)
                .Include(m => m.LostItem).ThenInclude(l => l.Campus)
                .Include(m => m.LostItem).ThenInclude(l => l.Category)
                .AsQueryable();


            if (userId.HasValue)
            {
                query = query.Where(m => (m.LostItem != null && m.LostItem.CreatedBy == userId) ||
                                         (m.FoundItem != null && m.FoundItem.CreatedBy == userId));
            }

            query = query.OrderByDescending(m => m.CreatedAt);

            int totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
