using DAL.Models;
using System.Threading.Tasks;

namespace DAL.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task UpdateAsync(User user);
        Task<List<User>> GetUsersByRoleAsync(int? roleId);
        Task<List<User>> GetUsersByStatusAsync(string status);
    }
}
