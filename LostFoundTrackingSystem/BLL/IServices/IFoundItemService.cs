using BLL.DTOs.FoundItemDTO;

namespace BLL.IServices
{
    public interface IFoundItemService
    {
        Task<List<FoundItemDto>> GetAllAsync();
        Task<FoundItemDto?> GetByIdAsync(int id);
        Task<FoundItemDto> CreateAsync(CreateFoundItemRequest request, int createdBy);
        Task<FoundItemDto> UpdateAsync(int id, UpdateFoundItemRequest request);
        Task DeleteAsync(int id);
        Task<List<FoundItemDto>> GetByCampusAsync(int campusId);
        Task<List<FoundItemDto>> GetByCategoryAsync(int categoryId);
        Task<List<FoundItemDto>> SearchByTitleAsync(string title);
    }
}