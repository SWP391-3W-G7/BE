using BLL.DTOs;
using BLL.IServices;
using DAL.IRepositories;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ItemActionLogService : IItemActionLogService
    {
        private readonly IItemActionLogRepository _repo;

        public ItemActionLogService(IItemActionLogRepository repo)
        {
            _repo = repo;
        }

        public async Task AddLogAsync(ItemActionLogDto logDto)
        {
            var log = new ItemActionLog
            {
                LostItemId = logDto.LostItemId,
                FoundItemId = logDto.FoundItemId,
                ClaimRequestId = logDto.ClaimRequestId,
                ActionType = logDto.ActionType,
                ActionDetails = logDto.ActionDetails,
                OldStatus = logDto.OldStatus,
                NewStatus = logDto.NewStatus,
                ActionDate = logDto.ActionDate ?? DateTime.UtcNow,
                PerformedBy = logDto.PerformedBy,
                CampusId = logDto.CampusId
            };
            await _repo.AddAsync(log);
        }

        public async Task<List<ItemActionLogDto>> GetLogsByFoundItemIdAsync(int foundItemId)
        {
            var logs = await _repo.GetByFoundItemIdAsync(foundItemId);
            return logs.Select(MapToDto).ToList();
        }

        public async Task<List<ItemActionLogDto>> GetLogsByLostItemIdAsync(int lostItemId)
        {
            var logs = await _repo.GetByLostItemIdAsync(lostItemId);
            return logs.Select(MapToDto).ToList();
        }

        public async Task<List<ItemActionLogDto>> GetLogsByClaimRequestIdAsync(int claimRequestId)
        {
            var logs = await _repo.GetByClaimRequestIdAsync(claimRequestId);
            return logs.Select(MapToDto).ToList();
        }

        private ItemActionLogDto MapToDto(ItemActionLog log)
        {
            return new ItemActionLogDto
            {
                ActionId = log.ActionId,
                LostItemId = log.LostItemId,
                FoundItemId = log.FoundItemId,
                ClaimRequestId = log.ClaimRequestId,
                ActionType = log.ActionType,
                ActionDetails = log.ActionDetails,
                OldStatus = log.OldStatus,
                NewStatus = log.NewStatus,
                ActionDate = log.ActionDate,
                PerformedBy = log.PerformedBy,
                PerformedByName = log.PerformedByNavigation?.FullName,
                CampusId = log.CampusId,
                CampusName = log.Campus?.CampusName
            };
        }
    }
}
