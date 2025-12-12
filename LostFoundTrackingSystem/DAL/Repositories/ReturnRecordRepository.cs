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
    public class ReturnRecordRepository : IReturnRecordRepository
    {
        private readonly LostFoundTrackingSystemContext _context;

        public ReturnRecordRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ReturnRecord record)
        {
            _context.ReturnRecords.Add(record);
            await _context.SaveChangesAsync();
        }

        public async Task<ReturnRecord?> GetByIdAsync(int id)
        {
            return await _context.ReturnRecords
                .Include(r => r.Receiver)
                .Include(r => r.Staff)
                .ThenInclude(s => s.User)
                .Include(r => r.FoundItem)
                .FirstOrDefaultAsync(r => r.ReturnId == id);
        }
        public async Task<List<ReturnRecord>> GetAllAsync()
        {
            return await _context.ReturnRecords
                .Include(r => r.Receiver)
                .Include(r => r.Staff)
                .ThenInclude(s => s.User)
                .Include(r => r.FoundItem)
                .ToListAsync();
        }
        public async Task<List<ReturnRecord>> GetByReceiverIdAsync(int receiverId)
        {
            return await _context.ReturnRecords
                .Where(r => r.ReceiverId == receiverId)
                .Include(r => r.Receiver)
                .Include(r => r.Staff)
                .ThenInclude(s => s.User)
                .Include(r => r.FoundItem)
                .ToListAsync();
        }
        public async Task<ReturnRecord> GetByLostItemIdAsync(int lostItemId)
        {
            return await _context.ReturnRecords
                .FirstOrDefaultAsync(r => r.LostItemId == lostItemId);
        }

        public async Task<ReturnRecord> GetByFoundItemIdAsync(int foundItemId)
        {
            return await _context.ReturnRecords
                .FirstOrDefaultAsync(r => r.FoundItemId == foundItemId);
        }
    }
}
