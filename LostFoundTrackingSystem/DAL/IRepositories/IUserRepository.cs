using DAL.Models;
using System.Threading.Tasks;

namespace DAL.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
    }
}
