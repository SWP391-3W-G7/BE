using BLL.DTOs.StaffDTO;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface IStaffService
    {
        Task<StaffWorkItemsDto> GetWorkItemsAsync(int campusId);
    }
}
