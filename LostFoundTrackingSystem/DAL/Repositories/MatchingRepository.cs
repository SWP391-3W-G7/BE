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

        public async Task<IEnumerable<ItemMatch>> GetMatchesForFoundItemAsync(int foundItemId)
        {
            return await _context.ItemMatches
                .Where(m => m.FoundItemId == foundItemId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ItemMatch>> GetMatchesForLostItemAsync(int lostItemId)
        {
            return await _context.ItemMatches
                .Where(m => m.LostItemId == lostItemId)
                .ToListAsync();
        }

        public async Task<IEnumerable<FoundItem>> GetPotentialMatchesAsync(LostItem lostItem)
        {
            var query = _context.FoundItems
                .Include(f => f.Images) // Include images for more detailed matching later if needed
                .Where(f => f.CategoryId == lostItem.CategoryId &&
                             f.CampusId == lostItem.CampusId &&
                             f.Status == "Available");

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
            return await _context.ItemMatches.FindAsync(matchId);
        }

        public async Task UpdateMatchAsync(ItemMatch itemMatch)
        {
            _context.ItemMatches.Update(itemMatch);
            await _context.SaveChangesAsync();
        }
    }
}
