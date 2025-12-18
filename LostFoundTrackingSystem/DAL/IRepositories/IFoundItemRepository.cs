using DAL.Models;

namespace DAL.IRepositories
{
    public interface IFoundItemRepository
    {
        Task<IEnumerable<FoundItem>> GetFoundItemsAsync(int? campusId, string status);
        Task<FoundItem?> GetByIdAsync(int id);
        Task AddAsync(FoundItem item);
        Task UpdateAsync(FoundItem item);
        Task DeleteAsync(FoundItem item);
        Task<List<FoundItem>> GetByCampusAsync(int campusId);
        Task<List<FoundItem>> GetByCampusAsync(int campusId, string status);
        Task<List<FoundItem>> GetByCategoryAsync(int categoryId);
        Task<List<FoundItem>> SearchByTitleAsync(string title);
        Task<List<FoundItem>> GetByCampusNameAndStatusAsync(string campusName, string status);
        Task<List<FoundItem>> GetByCreatedByAndStatusAsync(int createdById, string status);
        Task<IEnumerable<FoundItem>> GetByUserIdAsync(int userId);
        Task<(IEnumerable<FoundItem> Items, int TotalCount)> GetFoundItemsPagingAsync(int? campusId, string status, int pageNumber, int pageSize);
    }
}