using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.IRepositories
{
    public interface IClaimRequestRepository
    {
        Task<ClaimRequest?> GetByIdAsync(int id);
        Task<List<ClaimRequest>> GetByStudentIdAsync(int studentId);
        Task AddAsync(ClaimRequest request);
        Task UpdateAsync(ClaimRequest request);
        Task DeleteAsync(ClaimRequest request);
        Task<List<ClaimRequest>> GetAllAsync(ClaimStatus? status = null);
        Task<List<ClaimRequest>> GetByFoundItemIdAsync(int foundItemId);
    }
}
