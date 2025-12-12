using BLL.DTOs.AdminDTO;
using DAL.Models;

namespace BLL.IServices
{
    public interface IAdminService
    {
        Task<Campus> CreateCampusAsync(CreateCampusRequest request);
        Task AssignRoleAndCampusAsync(AssignRoleRequest request);
    }
}