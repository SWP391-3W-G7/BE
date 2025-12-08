using BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface ICampusService
    {
        Task<IEnumerable<CampusDto>> GetAllAsync();
    }
}
