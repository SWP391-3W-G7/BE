using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class FoundItemRepository : IFoundItemRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public FoundItemRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<List<FoundItem>> GetAllAsync()
        {
            return await _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .ToListAsync();
        }

        public async Task<FoundItem?> GetByIdAsync(int id)
        {
            return await _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .AsNoTracking() 
                .FirstOrDefaultAsync(f => f.FoundItemId == id);
        }

        public async Task AddAsync(FoundItem item)
        {
            _context.FoundItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FoundItem item)
        {
            var local = _context.Set<FoundItem>()
                .Local
                .FirstOrDefault(e => e.FoundItemId == item.FoundItemId);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(FoundItem item)
        {
            _context.FoundItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FoundItem>> GetByCampusAsync(int campusId)
        {
            return await _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .Where(f => f.CampusId == campusId)
                .ToListAsync();
        }

        public async Task<List<FoundItem>> GetByCategoryAsync(int categoryId)
        {
            return await _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .Where(f => f.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<List<FoundItem>> SearchByTitleAsync(string title)
        {
            return await _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .Where(f => f.Title.Contains(title))
                .ToListAsync();
        }
    }
}