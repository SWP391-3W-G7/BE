using DAL.Models;

namespace DAL.IRepositories
{
    public interface IReturnRecordRepository
    {
        Task AddAsync(ReturnRecord record);
        Task<ReturnRecord?> GetByIdAsync(int id);
        Task<List<ReturnRecord>> GetAllAsync();
        Task<List<ReturnRecord>> GetByReceiverIdAsync(int receiverId);
        Task<ReturnRecord> GetByLostItemIdAsync(int lostItemId);
        Task<ReturnRecord> GetByFoundItemIdAsync(int foundItemId);
        Task UpdateAsync(ReturnRecord record);
    }
}