using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.IRepositories
{
    public interface ILostItemRepository
    {
        Task<List<LostItem>> GetAllAsync();
        Task<LostItem?> GetByIdAsync(int id);
        Task AddAsync(LostItem item);
        Task UpdateAsync(LostItem item);
        Task DeleteAsync(LostItem item);
        Task<List<LostItem>> GetByCampusAsync(int campusId);
        Task<List<LostItem>> GetByCategoryAsync(int categoryId);
        Task<List<LostItem>> SearchByTitleAsync(string title);
        Task<List<LostItem>> GetByCreatedByAsync(int userId);
        Task<(List<LostItem> Items, int TotalCount)> GetLostItemsPagingAsync(int? campusId, string status, int pageNumber, int pageSize);
    }
}
