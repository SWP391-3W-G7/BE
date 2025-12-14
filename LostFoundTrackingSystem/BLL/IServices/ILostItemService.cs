using BLL.DTOs.LostItemDTO;

namespace BLL.IServices
{
    public interface ILostItemService
    {
        Task<List<LostItemDto>> GetAllAsync();
        Task<LostItemDto?> GetByIdAsync(int id);
        Task<LostItemDto> CreateAsync(CreateLostItemRequest request, int createdBy);
        Task<LostItemDto> UpdateAsync(int id, UpdateLostItemRequest request);
        Task DeleteAsync(int id);
        Task<List<LostItemDto>> GetByCampusAsync(int campusId);
        Task<List<LostItemDto>> GetByCategoryAsync(int categoryId);
        Task<List<LostItemDto>> SearchByTitleAsync(string title);
        Task<LostItemDto> UpdateStatusAsync(int lostItemId, UpdateLostItemStatusRequest request, int staffId);
    }
}
