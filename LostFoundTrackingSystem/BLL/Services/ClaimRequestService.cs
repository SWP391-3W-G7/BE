using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.Paging;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using BLL.DTOs; // Added to access ItemActionLogDto
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly ILostItemRepository _lostItemRepo;
        private readonly IMatchingRepository _matchingRepo;

        public ClaimRequestService(IClaimRequestRepository repo, IFoundItemRepository foundItemRepo, IImageRepository imageRepo, IImageService imageService, IReturnRecordRepository returnRecordRepo, INotificationService notifService, IItemActionLogService itemActionLogService, ILostItemRepository lostItemRepo, IMatchingRepository matchingRepo)
        {
            _repo = repo;
            _foundItemRepo = foundItemRepo;
            _imageRepo = imageRepo;
            _imageService = imageService;
            _returnRecordRepo = returnRecordRepo;
            _notifService = notifService;
            _itemActionLogService = itemActionLogService;
            _lostItemRepo = lostItemRepo;
            _matchingRepo = matchingRepo;
        }

        public async Task<ClaimRequestDto> CreateAsync(CreateClaimRequest request, int studentId)
        {
            var foundItem = await _foundItemRepo.GetByIdAsync(request.FoundItemId);
            if (foundItem == null) throw new Exception("Found item not found.");
            
            var lostItem = await _lostItemRepo.GetByIdAsync(request.LostItemId.Value);

            bool hasEvidence = !string.IsNullOrEmpty(request.EvidenceTitle) ||
                       !string.IsNullOrEmpty(request.EvidenceDescription) ||
                       (request.EvidenceImages != null && request.EvidenceImages.Count > 0);

            ClaimPriority priority = ClaimPriority.Low;
            if (request.LostItemId.HasValue && hasEvidence)
            {
                priority = ClaimPriority.High;
            }
            else if (request.LostItemId.HasValue && hasEvidence)
            {
                priority = ClaimPriority.Medium; 
            }
            else
            {
                priority = ClaimPriority.Low; 
            }

            var claimEntity = new ClaimRequest
            {
                FoundItemId = request.FoundItemId,
                LostItemId = request.LostItemId,
                StudentId = studentId,
                ClaimDate = DateTime.UtcNow,
                Status = ClaimStatus.Pending.ToString(),
                Priority = (int)priority
            };

            Evidence? evidenceEntity = null;
            if (hasEvidence)
            {
                evidenceEntity = new Evidence
                {
                    Title = request.EvidenceTitle ?? "No Title", 
                    Description = request.EvidenceDescription ?? "No Description",
                    CampusId = request.CampusId,
                    CreatedAt = DateTime.UtcNow,
                };
                claimEntity.Evidences.Add(evidenceEntity);
            }

            await _repo.AddAsync(claimEntity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                FoundItemId = foundItem.FoundItemId,
                ClaimRequestId = claimEntity.ClaimId,
                ActionType = "Created",
                ActionDetails = $"Claim created. Priority: {priority}. Status: {claimEntity.Status}.",
                NewStatus = claimEntity.Status,
                PerformedBy = studentId,
                CampusId = foundItem.CampusId
            });

            // Check for conflicts
            var allClaimsForFoundItem = await _repo.GetByFoundItemIdAsync(request.FoundItemId);
            bool hasMultipleClaims = allClaimsForFoundItem.Count > 1;
            bool hasConflictedClaims = allClaimsForFoundItem.Any(c => c.Status == ClaimStatus.Conflicted.ToString());

            if (hasMultipleClaims || hasConflictedClaims)
            {
                foreach (var claim in allClaimsForFoundItem)
                {
                    if (claim.Status == ClaimStatus.Pending.ToString())
                    {
                        string oldStatus = claim.Status;
                        claim.Status = ClaimStatus.Conflicted.ToString();
                        
                        await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                        {
                            ClaimRequestId = claim.ClaimId,
                            FoundItemId = claim.FoundItemId,
                            ActionType = "StatusUpdate",
                            ActionDetails = $"Claim request '{claim.ClaimId}' status automatically changed to 'Conflicted' due to multiple or existing conflicted claims.",
                            OldStatus = oldStatus,
                            NewStatus = claim.Status,
                            PerformedBy = studentId, // System action
                            CampusId = foundItem.CampusId
                        });
                    }
                }
                await _repo.SaveChangesAsync();
            }

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

        public async Task<List<ClaimRequestDto>> GetAllAsync(ClaimStatus? status = null)
        {
            var list = await _repo.GetAllAsync(status);
            var dtoList = new List<ClaimRequestDto>();
            foreach (var item in list)
            {
                dtoList.Add(await MapToDto(item));
            }
            return dtoList;
        }

        public async Task<PagedResponse<ClaimRequestDto>> GetAllPagingAsync(ClaimStatus? status, PagingParameters pagingParameters)
        {
            var (items, totalCount) = await _repo.GetAllPagingAsync(status, pagingParameters.PageNumber, pagingParameters.PageSize);
            
            var dtoList = new List<ClaimRequestDto>();
            foreach (var item in items)
            {
                dtoList.Add(await MapToDto(item));
            }

            return new PagedResponse<ClaimRequestDto>(dtoList, totalCount, pagingParameters.PageNumber, pagingParameters.PageSize);
        }

        public async Task<List<ClaimRequestDto>> GetMyClaimsAsync(int studentId)
        {
            var list = await _repo.GetByStudentIdAsync(studentId);
            var dtoList = new List<ClaimRequestDto>();
            foreach (var item in list)
            {
                dtoList.Add(await MapToDto(item));
            }
            return dtoList;
        }

        public async Task<ClaimRequestDto?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;
            return await MapToDto(item);
        }

        private async Task<ClaimRequestDto> MapToDto(ClaimRequest c)
        {
            var actionLogs = await _itemActionLogService.GetLogsByClaimRequestIdAsync(c.ClaimId);

            return new ClaimRequestDto
            {
                ClaimId = c.ClaimId,
                ClaimDate = c.ClaimDate,
                Status = c.Status,
                Priority = ((ClaimPriority)c.Priority).ToString(),
                FoundItemId = c.FoundItemId,
                FoundItemTitle = c.FoundItem?.Title,
                LostItemId = c.LostItemId,
                LostItemTitle = c.LostItem?.Title,
                StudentId = c.StudentId,
                StudentName = c.Student?.FullName,
                Evidences = c.Evidences.Select(e => new EvidenceDto
                {
                    EvidenceId = e.EvidenceId,
                    Title = e.Title,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    ImageUrls = e.Images.Select(i => i.ImageUrl).ToList()
                }).ToList(),
                ActionLogs = actionLogs
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

            // If the current claim is approved, reject all other pending/conflicted claims for the same found item
            if (status == ClaimStatus.Approved)
            {
                var otherClaims = (await _repo.GetByFoundItemIdAsync(entity.FoundItemId.Value))
                                    .Where(c => c.ClaimId != entity.ClaimId && (c.Status == ClaimStatus.Pending.ToString() || c.Status == ClaimStatus.Conflicted.ToString()))
                                    .ToList();

                foreach (var otherClaim in otherClaims)
                {
                    string otherClaimOldStatus = otherClaim.Status;
                    otherClaim.Status = ClaimStatus.Rejected.ToString();
                    await _repo.UpdateAsync(otherClaim);

                    await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                    {
                        ClaimRequestId = otherClaim.ClaimId,
                        FoundItemId = otherClaim.FoundItemId,
                        ActionType = "StatusUpdate",
                        ActionDetails = $"Claim request '{otherClaim.ClaimId}' for Found Item '{entity.FoundItem?.Title}' auto-rejected because claim '{entity.ClaimId}' was approved.",
                        OldStatus = otherClaimOldStatus,
                        NewStatus = otherClaim.Status,
                        PerformedBy = staffId, // Action performed by the staff who approved the main claim
                        CampusId = entity.FoundItem?.CampusId
                    });

                    // Notify student about rejected claim
                    if (otherClaim.StudentId.HasValue)
                    {
                                            string rejectedStudentUserId = otherClaim.StudentId.Value.ToString();
                                            string rejectedMessage = $"Your claim request (ID: {otherClaim.ClaimId}) for item '{entity.FoundItem?.Title}' has been rejected because another claim was approved.";
                                            try
                                            {
                                                await _notifService.SendNotificationAsync(rejectedStudentUserId, otherClaim.ClaimId, otherClaim.Status, rejectedMessage);
                                            }                        catch (Exception ex)
                                            {
                            Console.WriteLine($"Failed to send notification for rejected claim: {ex.Message}");
                        }
                    }
                }

                var matches = await _matchingRepo.GetMatchesForFoundItemAsync(entity.FoundItemId.Value);
                foreach (var match in matches)
                {
                    if (match.Status == "Pending" || match.Status == "Conflicted")
                    {
                        match.Status = "Dismissed";
                        match.MatchStatus = "Dismissed";
                        await _matchingRepo.UpdateMatchAsync(match);
                    }
                }
            }
            
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

                var foundItem = await _foundItemRepo.GetByIdAsync(entity.FoundItemId.Value);
                if (foundItem != null)
                {
                    foundItem.Status = FoundItemStatus.Returned.ToString();
                    await _foundItemRepo.UpdateAsync(foundItem);
                }
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

        public async Task RequestMoreEvidenceAsync(int claimId, string message, int staffId)
        {
            var claim = await _repo.GetByIdAsync(claimId);
            if (claim == null) throw new Exception("Claim request not found.");

            message += $"Your claim request (ID: {claim.FoundItem.Title}) need more evidences.";

            if (claim.StudentId.HasValue)
            {
                string studentUserId = claim.StudentId.Value.ToString();
                try
                {
                    await _notifService.SendNotificationAsync(studentUserId, claimId, claim.Status, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
            }

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                ClaimRequestId = claim.ClaimId,
                FoundItemId = claim.FoundItemId,
                ActionType = "EvidenceRequested",
                ActionDetails = $"Staff requested more evidence: {message}",
                PerformedBy = staffId,
                CampusId = claim.FoundItem?.CampusId
            });
        }


        public async Task ConflictClaimAsync(int claimId, int staffUserId)
        {
            var entity = await _repo.GetByIdAsync(claimId);
            if (entity == null) throw new Exception("Claim request not found.");

            string oldStatus = entity.Status;
            entity.Status = ClaimStatus.Conflicted.ToString();
            await _repo.UpdateAsync(entity);

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                ClaimRequestId = entity.ClaimId,
                FoundItemId = entity.FoundItemId,
                ActionType = "StatusUpdate",
                ActionDetails = $"Claim request '{entity.ClaimId}' status changed from '{oldStatus}' to '{entity.Status}'.",
                OldStatus = oldStatus,
                NewStatus = entity.Status,
                PerformedBy = staffUserId,
                CampusId = entity.FoundItem.CampusId
            });
        }
        
        public async Task AddEvidenceToClaimAsync(int claimId, AddEvidenceRequest request, int userId)
        {
            var claim = await _repo.GetByIdAsync(claimId);
            if (claim == null) throw new Exception("Claim request not found.");

            if (claim.StudentId != userId)
                throw new Exception("You are not authorized to add evidence to this claim.");

            // Can only add evidence if claim is Pending or Conflicted
            if (claim.Status != ClaimStatus.Pending.ToString() && claim.Status != ClaimStatus.Conflicted.ToString())
                throw new Exception("Cannot add evidence to a claim that has been processed.");

            var evidenceEntity = new Evidence
            {
                ClaimId = claimId,
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                // CampusId can be null or derived from FoundItem.CampusId if needed
            };

            claim.Evidences.Add(evidenceEntity);
            await _repo.UpdateAsync(claim); // Update to save the new evidence entity to the claim

            if (request.Images != null)
            {
                foreach (var file in request.Images)
                {
                    var url = await _imageService.UploadAsync(file);

                    await _imageRepo.AddAsync(new Image
                    {
                        EvidenceId = evidenceEntity.EvidenceId,
                        ImageUrl = url,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = userId
                    });
                }
            }
            
            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                ClaimRequestId = claim.ClaimId,
                FoundItemId = claim.FoundItemId,
                ActionType = "EvidenceAdded",
                ActionDetails = $"New evidence added to claim request '{claim.ClaimId}'.",
                PerformedBy = userId,
                CampusId = claim.FoundItem.CampusId
            });
        }
        public async Task UpdatePriorityAsync(int id, ClaimPriority priority)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Claim request not found");
            entity.Priority = (int)priority;
            await _repo.UpdateAsync(entity);
        }

        
        public async Task ScanForConflictingClaimsAsync()
        {
            var allClaims = await _repo.GetAllAsync();

            var claimsByFoundItem = allClaims.GroupBy(c => c.FoundItemId);

            foreach (var group in claimsByFoundItem)
            {
                if (!group.Key.HasValue) continue;

                bool hasMultipleClaims = group.Count() > 1;
                bool hasExistingConflict = group.Any(c => c.Status == ClaimStatus.Conflicted.ToString());

                if (hasMultipleClaims || hasExistingConflict)
                {
                    foreach (var claim in group)
                    {
                        if (claim.Status == ClaimStatus.Pending.ToString())
                        {
                            string oldStatus = claim.Status;
                            claim.Status = ClaimStatus.Conflicted.ToString();
                            
                            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                            {
                                ClaimRequestId = claim.ClaimId,
                                FoundItemId = claim.FoundItemId,
                                ActionType = "StatusUpdate",
                                ActionDetails = "Claim status automatically changed to 'Conflicted' due to a system scan for conflicting claims.",
                                OldStatus = oldStatus,
                                NewStatus = claim.Status,
                                                            PerformedBy = 1, // System action - Changed from 0 to 1
                                                            CampusId = claim.FoundItem?.CampusId
                                                        });                        }
                    }
                }
            }
            await _repo.SaveChangesAsync();
        }

        public async Task<PagedResponse<ClaimRequestDto>> GetClaimsByCampusAndStatusPagingAsync(int campusId, ClaimStatus status, PagingParameters pagingParameters)
        {
            var (items, totalCount) = await _repo.GetByCampusAndStatusPagingAsync(
                campusId,
                status.ToString(),
                pagingParameters.PageNumber,
                pagingParameters.PageSize
            );

            var dtoList = new List<ClaimRequestDto>();
            foreach (var item in items)
            {
                dtoList.Add(await MapToDto(item));
            }

            return new PagedResponse<ClaimRequestDto>(
                dtoList,
                totalCount,
                pagingParameters.PageNumber,
                pagingParameters.PageSize
            );
        }
        public async Task<ClaimRequestDto> ApproveClaimAsync(int id, ApproveClaimRequestDto request, int staffId)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Claim request not found");

            // 1. Validate Status
            if (entity.Status != ClaimStatus.Pending.ToString() && entity.Status != ClaimStatus.Conflicted.ToString())
                throw new Exception($"Cannot approve a claim that is in '{entity.Status}' status.");

            // 2. Validate Time (Ensure pickup is in the future)
            if (request.PickupTime <= DateTime.UtcNow)
                throw new Exception("Pickup time must be in the future.");

            string oldStatus = entity.Status;

            // 3. Update Status
            entity.Status = ClaimStatus.Approved.ToString();

            // 4. Auto-reject other claims for the same item (Logic copied from UpdateStatusAsync)
            var otherClaims = (await _repo.GetByFoundItemIdAsync(entity.FoundItemId.Value))
                                .Where(c => c.ClaimId != entity.ClaimId &&
                                           (c.Status == ClaimStatus.Pending.ToString() || c.Status == ClaimStatus.Conflicted.ToString()))
                                .ToList();

            foreach (var otherClaim in otherClaims)
            {
                string otherClaimOldStatus = otherClaim.Status;
                otherClaim.Status = ClaimStatus.Rejected.ToString();
                await _repo.UpdateAsync(otherClaim);

                // Log rejection
                await _itemActionLogService.AddLogAsync(new ItemActionLogDto
                {
                    ClaimRequestId = otherClaim.ClaimId,
                    FoundItemId = otherClaim.FoundItemId,
                    ActionType = "AutoRejected",
                    ActionDetails = $"Automatically rejected because claim #{entity.ClaimId} was approved.",
                    OldStatus = otherClaimOldStatus,
                    NewStatus = otherClaim.Status,
                    PerformedBy = staffId, // System action triggered by staff
                    CampusId = entity.FoundItem?.CampusId
                });

                // Notify rejected student
                if (otherClaim.StudentId.HasValue)
                {
                    string rejectedMsg = $"Your claim for '{entity.FoundItem?.Title}' was rejected because another claim was approved.";
                    await _notifService.SendNotificationAsync(otherClaim.StudentId.Value.ToString(), otherClaim.ClaimId, otherClaim.Status, rejectedMsg);
                }
            }

            var matches = await _matchingRepo.GetMatchesForFoundItemAsync(entity.FoundItemId.Value);
            foreach (var match in matches)
            {
                if (match.Status == "Pending" || match.Status == "Conflicted")
                {
                    match.Status = "Dismissed";
                    match.MatchStatus = "Dismissed";
                    await _matchingRepo.UpdateMatchAsync(match);
                }
            }

            // 5. Save the approved entity
            await _repo.UpdateAsync(entity);

            // 6. Log the Approval with Pickup Details (This serves as the record since we aren't changing DB schema)
            string logDetails = $"Claim Approved. Pickup Location: {request.PickupLocation}. " +
                                $"Time: {request.PickupTime:g}. " + // 'g' is general date/time pattern (short time)
                                $"Note: {request.AdminNote ?? "None"}";

            await _itemActionLogService.AddLogAsync(new ItemActionLogDto
            {
                ClaimRequestId = entity.ClaimId,
                FoundItemId = entity.FoundItemId,
                ActionType = "Approved",
                ActionDetails = logDetails,
                OldStatus = oldStatus,
                NewStatus = entity.Status,
                PerformedBy = staffId,
                CampusId = entity.FoundItem.CampusId
            });

            // 7. Notify the Student with Pickup Instructions
            string studentUserId = entity.StudentId.ToString();
            string notificationMessage = $"CONGRATS! Your claim for '{entity.FoundItem?.Title}' is APPROVED. " +
                                         $"Please pick it up at: {request.PickupLocation} " +
                                         $"on {request.PickupTime:dd/MM/yyyy HH:mm}. " +
                                         (string.IsNullOrEmpty(request.AdminNote) ? "" : $"Note: {request.AdminNote}");

            try
            {
                await _notifService.SendNotificationAsync(studentUserId, entity.ClaimId, entity.Status, notificationMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send pickup notification: {ex.Message}");
            }

            return await GetByIdAsync(entity.ClaimId);
        }
        public async Task<ClaimStatisticDto> GetClaimStatisticsAsync(int? campusId)
        {
            var rawData = await _repo.GetClaimStatusStatisticsAsync(campusId);

            var dto = new ClaimStatisticDto();

            int GetCount(ClaimStatus status) =>
                rawData.ContainsKey(status.ToString()) ? rawData[status.ToString()] : 0;

            dto.TotalPending = GetCount(ClaimStatus.Pending);
            dto.TotalApproved = GetCount(ClaimStatus.Approved);
            dto.TotalRejected = GetCount(ClaimStatus.Rejected);
            dto.TotalReturned = GetCount(ClaimStatus.Returned);
            dto.TotalConflicted = GetCount(ClaimStatus.Conflicted);

            dto.TotalClaims = dto.TotalPending + dto.TotalApproved + dto.TotalRejected +
                              dto.TotalReturned + dto.TotalConflicted;

            return dto;
        }
    }
}