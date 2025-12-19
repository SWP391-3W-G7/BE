using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.Paging;
using DAL.Models;

namespace BLL.IServices
{
    public interface IClaimRequestService
    {
        Task<List<ClaimRequestDto>> GetAllAsync(ClaimStatus? status = null);
        Task<PagedResponse<ClaimRequestDto>> GetAllPagingAsync(ClaimStatus? status, PagingParameters pagingParameters);
        Task<List<ClaimRequestDto>> GetMyClaimsAsync(int studentId);
        Task<ClaimRequestDto?> GetByIdAsync(int id);
        Task<ClaimRequestDto> CreateAsync(CreateClaimRequest request, int studentId);
        Task<ClaimRequestDto> UpdateAsync(int id, UpdateClaimRequest request, int userId);
        Task<ClaimRequestDto> UpdateStatusAsync(int id, ClaimStatus status, int staffId);
        Task ConflictClaimAsync(int claimId, int staffUserId);
        Task AddEvidenceToClaimAsync(int claimId, AddEvidenceRequest request, int userId);
        Task UpdatePriorityAsync(int id, ClaimPriority priority);
        Task RequestMoreEvidenceAsync(int claimId, string message, int staffId);
        Task ScanForConflictingClaimsAsync();
        Task<PagedResponse<ClaimRequestDto>> GetClaimsByCampusAndStatusPagingAsync(int campusId, ClaimStatus status, PagingParameters pagingParameters);
        Task<ClaimRequestDto> ApproveClaimAsync(int id, ApproveClaimRequestDto request, int staffId);
    }
}