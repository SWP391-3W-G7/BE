using DAL.IRepositories;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class CampusRepository : ICampusRepository
    {
        private readonly LostFoundTrackingSystemContext _context;
        public CampusRepository(LostFoundTrackingSystemContext context) => _context = context;

        public async Task AddAsync(Campus campus)
        {
            _context.Campuses.Add(campus);
            await _context.SaveChangesAsync();
        }

        public async Task<Campus?> GetByIdAsync(int id)
        {
            return await _context.Campuses.FindAsync(id);
        }
        public async Task<List<Campus>> GetAllAsync()
        {
            return await _context.Campuses.ToListAsync();
        }
    }
}
