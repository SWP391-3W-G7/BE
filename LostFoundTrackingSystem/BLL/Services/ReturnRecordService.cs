using System.Threading.Tasks;
using BLL.DTOs.ReturnRecordDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;

namespace BLL.Services
{
    public class ReturnRecordService : IReturnRecordService
    {
        private readonly IReturnRecordRepository _repo;
        private readonly IFoundItemRepository _foundItemRepo;
        private readonly ILostItemRepository _lostItemRepo;
        private readonly IStaffRepository _staffRepo;

        public ReturnRecordService(IReturnRecordRepository repo, IFoundItemRepository foundItemRepo, ILostItemRepository lostItemRepo, IStaffRepository staffRepo)
        {
            _repo = repo;
            _foundItemRepo = foundItemRepo;
            _lostItemRepo = lostItemRepo;
            _staffRepo = staffRepo;
        }

        public async Task<ReturnRecordDto> CreateReturnRecordAsync(CreateReturnRecordRequest request, int userId)
        {
            var staff = await _staffRepo.GetByUserIdAsync(userId);
            if (staff == null)
            {
                throw new Exception("Current user is not a valid Staff member.");
            }
            int realStaffId = staff.StaffId;

            var foundItem = await _foundItemRepo.GetByIdAsync(request.FoundItemId);
            if (foundItem == null)
                throw new Exception("Found item not found.");

            if (foundItem.Status == "Returned")
                throw new Exception("This found item has already been returned.");

            var existingFoundReturn = await _repo.GetByFoundItemIdAsync(request.FoundItemId);
            if (existingFoundReturn != null)
            {
                throw new Exception($"This found item has already been returned in Return Record #{existingFoundReturn.ReturnId}.");
            }

            LostItem lostItem = null;
            if (request.LostItemId.HasValue)
            {
                var existingReturn = await _repo.GetByLostItemIdAsync(request.LostItemId.Value);
                if (existingReturn != null)
                {
                    throw new Exception($"This lost item has already been returned in Return Record #{existingReturn.ReturnId}.");
                }
                lostItem = await _lostItemRepo.GetByIdAsync(request.LostItemId.Value);
                if (lostItem == null)
                    throw new Exception("Linked lost item not found.");

                if (lostItem.Status == "Returned")
                    throw new Exception("The linked lost item is already returned.");

            }

            var returnRecord = new ReturnRecord
            {
                FoundItemId = request.FoundItemId,
                LostItemId = request.LostItemId,
                ReceiverId = request.ReceiverId,
                StaffId = realStaffId,
                ReturnDate = request.ReturnDate ?? DateTime.UtcNow, 
                Note = request.Note
            };

            await _repo.AddAsync(returnRecord);

            foundItem.Status = "Returned";
            await _foundItemRepo.UpdateAsync(foundItem);

            if (lostItem != null)
            {
                lostItem.Status = "Returned";
                await _lostItemRepo.UpdateAsync(lostItem);
            }

            return await GetByIdAsync(returnRecord.ReturnId);
        }

        public async Task<List<ReturnRecordDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return MapToDtoList(list);
        }
        public async Task<List<ReturnRecordDto>> GetMyRecord(int receiverId)
        {
            var list = await _repo.GetByReceiverIdAsync(receiverId);
            return MapToDtoList(list);
        }
        public async Task<ReturnRecordDto> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null)
                throw new Exception("Return record not found.");
            return MapToDto(item);
        }

        private List<ReturnRecordDto> MapToDtoList(List<ReturnRecord> records)
        {
            return records.Select(MapToDto).ToList();
        }

        private ReturnRecordDto MapToDto(ReturnRecord r)
        {
            return new ReturnRecordDto
            {
                ReturnId = r.ReturnId,
                ReturnDate = r.ReturnDate,
                Note = r.Note,
                ReceiverId = r.ReceiverId,
                ReceiverName = r.Receiver?.FullName ?? "Unknown",
                StaffId = r.StaffId,
                StaffName = r.Staff?.User.FullName ?? "Unknown",
                FoundItemId = r.FoundItemId,
                LostItemId = r.LostItemId
            };
        }
    }
}