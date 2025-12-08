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

        public CampusRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Campus>> GetAllAsync()
        {
            return await _context.Campuses.ToListAsync();
        }
    }
}
