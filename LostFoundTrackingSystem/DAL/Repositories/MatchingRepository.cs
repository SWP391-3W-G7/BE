using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit; // Added

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
                             (f.Status == FoundItemStatus.Stored.ToString()));

            // Build dynamic text matching conditions with OR logic
            var titleDescriptionPredicate = PredicateBuilder.False<FoundItem>();

            if (!string.IsNullOrWhiteSpace(lostItem.Title))
            {
                var lowerLostItemTitle = lostItem.Title.ToLower();
                titleDescriptionPredicate = titleDescriptionPredicate.Or(f =>
                    f.Title.ToLower().Contains(lowerLostItemTitle) ||
                    f.Description.ToLower().Contains(lowerLostItemTitle)
                );
            }

            if (!string.IsNullOrWhiteSpace(lostItem.Description))
            {
                var lowerLostItemDescription = lostItem.Description.ToLower();
                titleDescriptionPredicate = titleDescriptionPredicate.Or(f =>
                    f.Description.ToLower().Contains(lowerLostItemDescription) ||
                    f.Title.ToLower().Contains(lowerLostItemDescription)
                );
            }

            // Only apply the title/description predicate if it has any conditions
            if (titleDescriptionPredicate.Parameters.Any())
            {
                query = query.Where(titleDescriptionPredicate);
            }

            return await query.ToListAsync();
        }

        public async Task<ItemMatch> GetMatchByIdAsync(int matchId)
        {
            return await _context.ItemMatches
                .Include(m => m.LostItem)
                    .ThenInclude(li => li.Campus)
                .Include(m => m.LostItem)
                    .ThenInclude(li => li.Category)
                .Include(m => m.LostItem)
                    .ThenInclude(li => li.Images)
                .Include(m => m.FoundItem)
                    .ThenInclude(fi => fi.Campus)
                .Include(m => m.FoundItem)
                    .ThenInclude(fi => fi.Category)
                .Include(m => m.FoundItem)
                    .ThenInclude(fi => fi.Images)
                .Include(m => m.FoundItem)
                    .ThenInclude(fi => fi.ClaimRequests)
                        .ThenInclude(cr => cr.Evidences)
                            .ThenInclude(e => e.Images)
                .Include(m => m.FoundItem)
                    .ThenInclude(fi => fi.ClaimRequests)
                        .ThenInclude(cr => cr.Student)
                .Include(m => m.CreatedByNavigation)
                    .ThenInclude(cbn => cbn.Role)
                .Include(m => m.CreatedByNavigation)
                    .ThenInclude(cbn => cbn.Campus)
                .FirstOrDefaultAsync(m => m.MatchId == matchId);
        }

        public async Task UpdateMatchAsync(ItemMatch itemMatch)
        {
            var existingMatch = await _context.ItemMatches.FindAsync(itemMatch.MatchId);
            if (existingMatch != null)
            {
                existingMatch.MatchStatus = itemMatch.MatchStatus;
                existingMatch.Status = itemMatch.Status;
                
                _context.Entry(existingMatch).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
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
