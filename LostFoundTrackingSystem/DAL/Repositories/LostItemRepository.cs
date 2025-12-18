using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class LostItemRepository : ILostItemRepository
    {
        private readonly LostFoundTrackingSystemContext _context;
        public LostItemRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }
        public async Task<List<LostItem>> GetAllAsync()
        {
            return await _context.LostItems
                .Include(l => l.Images)
                .Include(l => l.Campus)
                .Include(l => l.Category)
                .ToListAsync();
        }

        public async Task<LostItem?> GetByIdAsync(int id)
        {
            return await _context.LostItems
                .Include(l => l.Images)
                .Include(l => l.Campus)
                .Include(l => l.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LostItemId == id);
        }

        public async Task AddAsync(LostItem item)
        {
            _context.LostItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LostItem item)
        {
            var local = _context.Set<LostItem>()
                .Local
                .FirstOrDefault(e => e.LostItemId == item.LostItemId);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LostItem item)
        {
            _context.LostItems.Remove(item);
            await _context.SaveChangesAsync();
        }
        public async Task<List<LostItem>> GetByCampusAsync(int campusId)
        {
            return await _context.LostItems
                .Include(l => l.Images)
                .Include(l => l.Campus)
                .Include(l => l.Category)
                .Where(l => l.CampusId == campusId)
                .ToListAsync();
        }
        public async Task<List<LostItem>> GetByCategoryAsync(int categoryId)
        {
            return await _context.LostItems
                .Include(l => l.Images)
                .Include(l => l.Campus)
                .Include(l => l.Category)
                .Where(l => l.CategoryId == categoryId)
                .ToListAsync();
        }
        public async Task<List<LostItem>> SearchByTitleAsync(string title)
        {
            return await _context.LostItems
                .Include(l => l.Images)
                .Include(l => l.Campus)
                .Include(l => l.Category)
                .Where(l => l.Title.Contains(title))
                .ToListAsync();
        }

        public async Task<List<LostItem>> GetByCreatedByAsync(int userId)
        {
            return await _context.LostItems
                .Include(l => l.Images)
                .Include(l => l.Campus)
                .Include(l => l.Category)
                .Where(l => l.CreatedBy == userId)
                .ToListAsync();
        }

        public async Task<(List<LostItem> Items, int TotalCount)> GetLostItemsPagingAsync(int? campusId, string status, int pageNumber, int pageSize)
        {
            var query = _context.LostItems
                .Include(l => l.Images)
                .Include(l => l.Campus)
                .Include(l => l.Category)
                .AsQueryable();

            if (campusId.HasValue)
            {
                query = query.Where(l => l.CampusId == campusId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(l => l.Status == status);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
