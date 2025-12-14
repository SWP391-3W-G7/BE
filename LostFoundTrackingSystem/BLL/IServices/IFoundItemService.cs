using BLL.DTOs.FoundItemDTO;

namespace BLL.IServices
{
    public interface IFoundItemService
    {
        Task<List<FoundItemDto>> GetAllAsync();
        Task<FoundItemDto?> GetByIdAsync(int id);
        Task<FoundItemDto> CreateAsync(CreateFoundItemRequest request, int createdBy, string initialStatus = null);
        Task<FoundItemDto> UpdateAsync(int id, UpdateFoundItemRequest request);
        Task DeleteAsync(int id);
        Task<List<FoundItemDto>> GetByCampusAsync(int campusId);
        Task<List<FoundItemDto>> GetByCampusAsync(int campusId, string status);
        Task<List<FoundItemDto>> GetByCategoryAsync(int categoryId);
        Task<List<FoundItemDto>> SearchByTitleAsync(string title);
        Task<FoundItemDetailsDto> GetFoundItemDetailsAsync(int foundItemId);
        Task<FoundItemDto> UpdateStatusAsync(int id, UpdateFoundItemStatusRequest request, int staffId);
    }
}