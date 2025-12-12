using DAL.IRepositories;
using DAL.Models;

namespace DAL.Repositories
{
    public class EvidenceRepository : IEvidenceRepository
    {
        private readonly LostFoundTrackingSystemContext _context;
        public EvidenceRepository(LostFoundTrackingSystemContext context) => _context = context;

        public async Task AddAsync(Evidence evidence)
        {
            _context.Evidences.Add(evidence);
            await _context.SaveChangesAsync();
        }
    }
}