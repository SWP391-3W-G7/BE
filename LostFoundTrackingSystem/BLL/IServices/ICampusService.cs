using BLL.DTOs.CampusDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.IServices
{
    public interface ICampusService
    {
        Task<CampusDto> CreateAsync(CreateCampusDto dto);
        Task<IEnumerable<CampusDto>> GetAllAsync();
        Task<CampusDto?> GetByIdAsync(int id);
        Task UpdateAsync(int id, UpdateCampusDto dto);
        Task DeleteAsync(int id);
    }
}
