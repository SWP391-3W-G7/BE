using DAL.Models;

namespace DAL.IRepositories
{
    public interface IFoundItemRepository
    {
        Task<List<FoundItem>> GetAllAsync();
        Task<FoundItem?> GetByIdAsync(int id);
        Task AddAsync(FoundItem item);
        Task UpdateAsync(FoundItem item);
        Task DeleteAsync(FoundItem item);
        Task<List<FoundItem>> GetByCampusAsync(int campusId);
        Task<List<FoundItem>> GetByCategoryAsync(int categoryId);
        Task<List<FoundItem>> SearchByTitleAsync(string title);
    }
}