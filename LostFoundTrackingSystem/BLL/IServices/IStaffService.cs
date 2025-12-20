using BLL.DTOs.Paging;
using BLL.DTOs.StaffDTO;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IStaffService
    {
        Task<StaffWorkItemsDto> GetWorkItemsAsync(int campusId, PagingParameters pagingParameters);
        Task RequestItemDropOffAsync(int foundItemId, RequestDropOffDto request, int staffId);
    }
}

