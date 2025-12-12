using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public StaffRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<Staff?> GetByUserIdAsync(int userId)
        {
            return await _context.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        public async Task AddAsync(Staff staff)
        {
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();
        }
    }
}