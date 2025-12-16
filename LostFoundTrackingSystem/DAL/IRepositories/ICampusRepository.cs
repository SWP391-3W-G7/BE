using DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.IRepositories
{
    public interface ICampusRepository
    {
        Task AddAsync(Campus campus);
        Task<Campus?> GetByIdAsync(int id);
        Task<List<Campus>> GetAllAsync();
        Task UpdateAsync(Campus campus);
        Task DeleteAsync(Campus campus);
    }
}
