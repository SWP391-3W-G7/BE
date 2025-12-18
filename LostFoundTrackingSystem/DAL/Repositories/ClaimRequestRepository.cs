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
    public class ClaimRequestRepository : IClaimRequestRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public ClaimRequestRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<List<ClaimRequest>> GetAllAsync(ClaimStatus? status = null)
        {
            var query = _context.ClaimRequests
                .Include(c => c.FoundItem)
                .Include(c => c.Student)
                .Include(c => c.LostItem)
                .Include(c => c.Evidences).ThenInclude(e => e.Images)
                .AsQueryable();
            if (status.HasValue)
            {
                string statusStr = status.ToString();
                query = query.Where(c => c.Status == statusStr);
            }
            return await query.ToListAsync();
        }

        public async Task<ClaimRequest?> GetByIdAsync(int id)
        {
            return await _context.ClaimRequests
                .Include(c => c.FoundItem)
                .Include(c => c.Student)
                .Include(c => c.LostItem)
                .Include(c => c.Evidences).ThenInclude(e => e.Images)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
        }

        public async Task<List<ClaimRequest>> GetByStudentIdAsync(int studentId)
        {
            return await _context.ClaimRequests
                .Include(c => c.FoundItem)
                .Include(c => c.LostItem)
                .Include(c => c.Evidences).ThenInclude(e => e.Images)
                .Where(c => c.StudentId == studentId)
                .ToListAsync();
        }

        public async Task AddAsync(ClaimRequest request)
        {
            _context.ClaimRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ClaimRequest request)
        {
            _context.ClaimRequests.Update(request);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(ClaimRequest request)
        {
            _context.ClaimRequests.Remove(request);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ClaimRequest>> GetByFoundItemIdAsync(int foundItemId)
        {
            return await _context.ClaimRequests
                .Where(c => c.FoundItemId == foundItemId)
                .Include(c => c.FoundItem)
                .Include(c => c.Student)
                .Include(c => c.LostItem)
                .Include(c => c.Evidences).ThenInclude(e => e.Images)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
