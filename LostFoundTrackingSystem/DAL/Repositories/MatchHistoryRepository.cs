using DAL.IRepositories;
using DAL.Models;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class MatchHistoryRepository : IMatchHistoryRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public MatchHistoryRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MatchHistory matchHistory)
        {
            await _context.MatchHistories.AddAsync(matchHistory);
            await _context.SaveChangesAsync();
        }
    }
}
