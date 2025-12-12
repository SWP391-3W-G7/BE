using DAL.Models;

namespace DAL.IRepositories
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByUserIdAsync(int userId);
        Task AddAsync(Staff staff);
    }
}