using BLL.DTOs.CampusDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
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
        public async Task<CampusDto?> GetByIdAsync(int id)
        {
            var c = await _campusRepository.GetByIdAsync(id);
            if (c == null) return null;

            return new CampusDto
            {
                CampusId = c.CampusId,
                CampusName = c.CampusName,
                Address = c.Address,
                StorageLocation = c.StorageLocation
            };
        }
        public async Task<CampusDto> CreateAsync(CreateCampusDto dto)
        {
            var campus = new Campus
            {
                CampusName = dto.CampusName,
                Address = dto.Address,
                StorageLocation = dto.StorageLocation
            };

            await _campusRepository.AddAsync(campus);

            return new CampusDto
            {
                CampusId = campus.CampusId,
                CampusName = campus.CampusName,
                Address = campus.Address,
                StorageLocation = campus.StorageLocation
            };
        }
        public async Task UpdateAsync(int id, UpdateCampusDto dto)
        {
            var campus = await _campusRepository.GetByIdAsync(id);
            if (campus == null) throw new Exception("Campus not found");

            if (!string.IsNullOrEmpty(dto.CampusName)) campus.CampusName = dto.CampusName;
            if (!string.IsNullOrEmpty(dto.Address)) campus.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.StorageLocation)) campus.StorageLocation = dto.StorageLocation;

            await _campusRepository.UpdateAsync(campus);
        }
        public async Task DeleteAsync(int id)
        {
            var campus = await _campusRepository.GetByIdAsync(id);
            if (campus == null) throw new Exception("Campus not found");
            
            if (campus.FoundItems.Any() || campus.LostItems.Any()) throw new Exception("Cannot delete campus containing items.");

            if(campus.Users.Any()) throw new Exception("Cannot delete campus containing users.");

            if(campus.Evidences.Any()) throw new Exception("Cannot delete campus containing evidences.");

            if(campus.ItemActionLogs.Any()) throw new Exception("Cannot delete campus containing item action logs.");

            await _campusRepository.DeleteAsync(campus);
        }
    }
}
