using BLL.DTOs.ClaimRequestDTO;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using BLL.DTOs; // Added to access ItemActionLogDto

namespace BLL.Services
{
    public class ClaimRequestService : IClaimRequestService
    {
        private readonly IClaimRequestRepository _repo;
        private readonly IFoundItemRepository _foundItemRepo;
        private readonly IImageRepository _imageRepo;
        private readonly IImageService _imageService;
        private readonly IReturnRecordRepository _returnRecordRepo;
        private readonly INotificationService _notifService;
        private readonly IItemActionLogService _itemActionLogService;

        public ClaimRequestService(IClaimRequestRepository repo, IFoundItemRepository foundItemRepo, IImageRepository imageRepo, IImageService imageService, IReturnRecordRepository returnRecordRepo, INotificationService notifService, IItemActionLogService itemActionLogService)
        {
            _repo = repo;
            _foundItemRepo = foundItemRepo;
            _imageRepo = imageRepo;
            _imageService = imageService;
            _returnRecordRepo = returnRecordRepo;
            _notifService = notifService;
            _itemActionLogService = itemActionLogService;
        }

        public async Task<ClaimRequestDto> CreateAsync(CreateClaimRequest request, int studentId)
        {
            var foundItem = await _foundItemRepo.GetByIdAsync(request.FoundItemId);
            if (foundItem == null) throw new Exception("Found item not found.");

            var claimEntity = new ClaimRequest
            {
                FoundItemId = request.FoundItemId,
                StudentId = studentId,
                ClaimDate = DateTime.UtcNow,
                Status = ClaimStatus.Pending.ToString()
            };

            var evidenceEntity = new Evidence
            {
                Title = request.EvidenceTitle,
                Description = request.EvidenceDescription,
                CampusId = request.CampusId,
                CreatedAt = DateTime.UtcNow,
            };

            claimEntity.Evidences.Add(evidenceEntity);

            await _repo.AddAsync(claimEntity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                FoundItemId = foundItem.FoundItemId,
                ClaimRequestId = claimEntity.ClaimId,
                ActionType = "Created",
                ActionDetails = $"Claim request for found item '{foundItem.Title}' created with status '{claimEntity.Status}'.",
                NewStatus = claimEntity.Status,
                PerformedBy = studentId,
                CampusId = foundItem.CampusId
            });

            if (request.EvidenceImages != null)
            {
                foreach (var file in request.EvidenceImages)
                {
                    var url = await _imageService.UploadAsync(file);

                    await _imageRepo.AddAsync(new Image
                    {
                        EvidenceId = evidenceEntity.EvidenceId, 
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = studentId
                    });
                }
            }

            return await GetByIdAsync(claimEntity.ClaimId);
        }

        public async Task<List<ClaimRequestDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return MapToDtoList(list);
        }

        public async Task<List<ClaimRequestDto>> GetMyClaimsAsync(int studentId)
        {
            var list = await _repo.GetByStudentIdAsync(studentId);
            return MapToDtoList(list);
        }

        public async Task<ClaimRequestDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;
            return MapToDto(item);
        }

        private List<ClaimRequestDto> MapToDtoList(List<ClaimRequest> items)
        {
            return items.Select(MapToDto).ToList();
        }

        private ClaimRequestDto MapToDto(ClaimRequest c)
        {
            return new ClaimRequestDto
            {
                ClaimId = c.ClaimId,
                ClaimDate = c.ClaimDate,
                Status = c.Status,
                FoundItemId = c.FoundItemId,
                FoundItemTitle = c.FoundItem?.Title,
                StudentId = c.StudentId,
                StudentName = c.Student?.FullName,
                Evidences = c.Evidences.Select(e => new EvidenceDto
                {
                    EvidenceId = e.EvidenceId,
                    Title = e.Title,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    ImageUrls = e.Images.Select(i => i.ImageUrl).ToList()
                }).ToList()
            };
        }
        public async Task<ClaimRequestDto> UpdateAsync(int id, UpdateClaimRequest request, int userId)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Claim request not found");

            if (entity.StudentId != userId)
                throw new Exception("You are not authorized to update this claim.");

            if (entity.Status != ClaimStatus.Pending.ToString())
                throw new Exception("Cannot update a claim that has been processed.");

            string oldStatus = entity.Status;

            var evidence = entity.Evidences.FirstOrDefault();
            if (evidence != null)
            {
                evidence.Title = request.EvidenceTitle;
                evidence.Description = request.EvidenceDescription;
            }

            entity.ClaimDate = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                ClaimRequestId = entity.ClaimId,
                FoundItemId = entity.FoundItemId,
                ActionType = "Updated",
                ActionDetails = $"Claim request '{entity.ClaimId}' updated. Status changed from '{oldStatus}' to '{entity.Status}'.",
                OldStatus = oldStatus,
                NewStatus = entity.Status,
                PerformedBy = userId,
                CampusId = entity.FoundItem.CampusId
            });

            if (request.NewImages != null && evidence != null)
            {
                foreach (var file in request.NewImages)
                {
                    var url = await _imageService.UploadAsync(file);
                    await _imageRepo.AddAsync(new Image
                    {
                        EvidenceId = evidence.EvidenceId,
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = userId
                    });
                }
            }

            return await GetByIdAsync(entity.ClaimId);
        }

        public async Task<ClaimRequestDto> UpdateStatusAsync(int id, ClaimStatus status, int staffId)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Claim request not found");

            string oldStatus = entity.Status;
            entity.Status = status.ToString();

            if (entity.Status.ToString() == ClaimStatus.Returned.ToString())
            {
                var returnRecord = new ReturnRecord
                {
                    FoundItemId = entity.FoundItemId,
                    ReceiverId = entity.StudentId,
                    StaffUserId = staffId,
                    ReturnDate = DateTime.UtcNow
                };
                await _returnRecordRepo.AddAsync(returnRecord);
            }

            await _repo.UpdateAsync(entity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                ClaimRequestId = entity.ClaimId,
                FoundItemId = entity.FoundItemId,
                ActionType = "StatusUpdate",
                ActionDetails = $"Claim request '{entity.ClaimId}' status changed from '{oldStatus}' to '{entity.Status}'.",
                OldStatus = oldStatus,
                NewStatus = entity.Status,
                PerformedBy = staffId,
                CampusId = entity.FoundItem.CampusId
            });

            string studentUserId = entity.StudentId.ToString();
            string message = $"Your claim request (ID: {entity.FoundItem.Title}) status has been updated to {entity.Status}.";
            try
            {
                await _notifService.SendNotificationAsync(studentUserId, entity.ClaimId, entity.Status, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            return await GetByIdAsync(entity.ClaimId);
        }
    }
}