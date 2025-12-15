using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public UserRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Campus)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Campus)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }
        public async Task UpdateAsync(User user)
        {
            user.Campus = null;
            user.Role = null;

            var entry = _context.Entry(user);
            if (entry.State == EntityState.Detached)
            {
                _context.Users.Attach(user);
            }
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<List<User>> GetUsersByRoleAsync(int? roleId)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Campus)
                .Where(u => u.RoleId != 4);

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId);
            }

            return await query.ToListAsync();
        }
    }
}
