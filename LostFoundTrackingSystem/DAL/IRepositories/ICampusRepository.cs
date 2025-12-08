using DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.IRepositories
{
    public interface ICampusRepository
    {
        Task<IEnumerable<Campus>> GetAllAsync();
    }
}
