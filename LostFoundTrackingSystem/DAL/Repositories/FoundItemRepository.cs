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

        public async Task<IEnumerable<FoundItem>> GetFoundItemsAsync(int? campusId, string status)
        {
            var query = _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .AsQueryable();

            if (campusId.HasValue)
            {
                query = query.Where(f => f.CampusId == campusId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(f => f.Status == status);
            }

            return await query.ToListAsync();
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

        public async Task<List<FoundItem>> GetByCampusAsync(int campusId, string status)
        {
            return await _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .Where(f => f.CampusId == campusId && f.Status == status)
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
        public async Task<List<FoundItem>> GetByCampusNameAndStatusAsync(string campusName, string status)
        {
            return await _context.FoundItems
                .Include(f => f.Images)
                .Include(f => f.Campus)
                .Include(f => f.Category)
                .Where(f => f.Campus != null && f.Campus.CampusName == campusName && f.Status == status)
                .ToListAsync();
        }
                public async Task<List<FoundItem>> GetByCreatedByAndStatusAsync(int createdById, string status)
                {
                    return await _context.FoundItems
                        .Include(f => f.Images)
                        .Include(f => f.Campus)
                        .Include(f => f.Category)
                        .Where(f => f.CreatedBy == createdById && f.Status == status)
                        .ToListAsync();
                }
        
                public async Task<IEnumerable<FoundItem>> GetByUserIdAsync(int userId)
                {
                    return await _context.FoundItems
                        .Include(f => f.Images)
                        .Include(f => f.Campus)
                        .Include(f => f.Category)
                        .Where(f => f.CreatedBy == userId)
                        .ToListAsync();
                }
            }
        }
        