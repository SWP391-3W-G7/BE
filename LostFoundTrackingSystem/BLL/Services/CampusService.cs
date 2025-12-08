using BLL.DTOs;
using BLL.IServices;
using DAL.IRepositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CampusService : ICampusService
    {
        private readonly ICampusRepository _campusRepository;

        public CampusService(ICampusRepository campusRepository)
        {
            _campusRepository = campusRepository;
        }

        public async Task<IEnumerable<CampusDto>> GetAllAsync()
        {
            var campuses = await _campusRepository.GetAllAsync();
            return campuses.Select(c => new CampusDto
            {
                CampusId = c.CampusId,
                CampusName = c.CampusName,
                Address = c.Address,
                StorageLocation = c.StorageLocation
            });
        }
    }
}
