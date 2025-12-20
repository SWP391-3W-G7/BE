using BLL.DTOs.LostItemDTO;
using BLL.DTOs.Paging;

namespace BLL.IServices
{
    public interface ILostItemService
    {
        Task<List<LostItemDto>> GetAllAsync();
        Task<PagedResponse<LostItemDto>> GetAllPagingAsync(LostItemFilterDto filter, PagingParameters pagingParameters);
        Task<LostItemDto?> GetByIdAsync(int id);
        Task<LostItemDto> CreateAsync(CreateLostItemRequest request, int createdBy);
        Task<LostItemDto> UpdateAsync(int id, UpdateLostItemRequest request);
        Task DeleteAsync(int id);
        Task<List<LostItemDto>> GetByCampusAsync(int campusId);
        Task<List<LostItemDto>> GetByCategoryAsync(int categoryId);
        Task<List<LostItemDto>> SearchByTitleAsync(string title);
        Task<List<LostItemDto>> GetMyLostItemsAsync(int userId);
        Task<LostItemDto> UpdateStatusAsync(int lostItemId, UpdateLostItemStatusRequest request, int staffId);
        Task<TopCampusStatDto?> GetCampusWithMostLostItemsAsync();
        Task<TopUserLostItemDto?> GetTopLostItemUserAsync(int? campusId);
        Task<LostItemStatisticDto> GetLostItemStatisticsAsync(int? campusId);
    }
}
