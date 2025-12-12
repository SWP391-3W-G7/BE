using BLL.DTOs.ReturnRecordDTO;

namespace BLL.IServices
{
    public interface IReturnRecordService
    {
        Task<ReturnRecordDto> CreateReturnRecordAsync(CreateReturnRecordRequest request, int staffId);
        Task<List<ReturnRecordDto>> GetAllAsync();
        Task<ReturnRecordDto> GetByIdAsync(int returnId);
        Task<List<ReturnRecordDto>> GetMyRecord(int receiverId);
    }
}