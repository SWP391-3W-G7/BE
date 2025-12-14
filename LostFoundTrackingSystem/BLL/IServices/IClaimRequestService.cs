using BLL.DTOs.ClaimRequestDTO;
using DAL.Models;

namespace BLL.IServices
{
    public interface IClaimRequestService
    {
        Task<List<ClaimRequestDto>> GetAllAsync();
        Task<List<ClaimRequestDto>> GetMyClaimsAsync(int studentId);
        Task<ClaimRequestDto?> GetByIdAsync(int id);
        Task<ClaimRequestDto> CreateAsync(CreateClaimRequest request, int studentId);
        Task<ClaimRequestDto> UpdateAsync(int id, UpdateClaimRequest request, int userId);
        Task<ClaimRequestDto> UpdateStatusAsync(int id, ClaimStatus status, int staffId);
        Task ConflictClaimAsync(int claimId, int staffUserId);
        Task AddEvidenceToClaimAsync(int claimId, AddEvidenceRequest request, int userId);
    }
}