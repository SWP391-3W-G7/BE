using System.Threading.Tasks;
using BLL.DTOs.ReturnRecordDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using BLL.DTOs.FoundItemDTO; // Added
using BLL.DTOs.LostItemDTO; // Added

namespace BLL.Services
{
    public class ReturnRecordService : IReturnRecordService
    {
        private readonly IReturnRecordRepository _repo;
        private readonly IFoundItemRepository _foundItemRepo;
        private readonly ILostItemRepository _lostItemRepo;
        private readonly IFoundItemService _foundItemService; // Injected
        private readonly ILostItemService _lostItemService; // Injected

        public ReturnRecordService(IReturnRecordRepository repo, IFoundItemRepository foundItemRepo, ILostItemRepository lostItemRepo, IFoundItemService foundItemService, ILostItemService lostItemService)
        {
            _repo = repo;
            _foundItemRepo = foundItemRepo;
            _lostItemRepo = lostItemRepo;
            _foundItemService = foundItemService;
            _lostItemService = lostItemService;
        }

        public async Task<ReturnRecordDto> CreateReturnRecordAsync(CreateReturnRecordRequest request, int userId)
        {
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
                StaffUserId = userId,
                ReturnDate = request.ReturnDate ?? DateTime.UtcNow, 
                Note = request.Note
            };

            await _repo.AddAsync(returnRecord);

            await _foundItemService.UpdateStatusAsync(foundItem.FoundItemId, new UpdateFoundItemStatusRequest { Status = FoundItemStatus.Returned.ToString() }, userId);

            if (lostItem != null)
            {
                await _lostItemService.UpdateStatusAsync(lostItem.LostItemId, new UpdateLostItemStatusRequest { Status = LostItemStatus.Returned.ToString() }, userId);
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
                StaffId = r.StaffUserId,
                StaffName = r.StaffUser?.FullName ?? "Unknown",
                FoundItemId = r.FoundItemId,
                LostItemId = r.LostItemId
            };
        }

        public async Task<ReturnRecordDto> UpdateReturnRecordAsync(int returnId, UpdateReturnRecordRequest request)
        {
            var entity = await _repo.GetByIdAsync(returnId);
            if (entity == null)
                throw new Exception("Return record not found.");

            entity.Note = request.Note;

            await _repo.UpdateAsync(entity);

            return await GetByIdAsync(entity.ReturnId);
        }
    }
}