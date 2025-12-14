using BLL.DTOs;
using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/item-action-logs")]
    [Authorize(Roles = "Staff,Admin")]
    public class ItemActionLogsController : ControllerBase
    {
        private readonly IItemActionLogService _itemActionLogService;

        public ItemActionLogsController(IItemActionLogService itemActionLogService)
        {
            _itemActionLogService = itemActionLogService;
        }

        [HttpGet("found-item/{foundItemId}")]
        public async Task<ActionResult<List<ItemActionLogDto>>> GetLogsByFoundItemId(int foundItemId)
        {
            var logs = await _itemActionLogService.GetLogsByFoundItemIdAsync(foundItemId);
            if (logs == null || !logs.Any())
            {
                return NotFound($"No action logs found for Found Item ID: {foundItemId}");
            }
            return Ok(logs);
        }

        [HttpGet("lost-item/{lostItemId}")]
        public async Task<ActionResult<List<ItemActionLogDto>>> GetLogsByLostItemId(int lostItemId)
        {
            var logs = await _itemActionLogService.GetLogsByLostItemIdAsync(lostItemId);
            if (logs == null || !logs.Any())
            {
                return NotFound($"No action logs found for Lost Item ID: {lostItemId}");
            }
            return Ok(logs);
        }

        [HttpGet("claim-request/{claimRequestId}")]
        public async Task<ActionResult<List<ItemActionLogDto>>> GetLogsByClaimRequestId(int claimRequestId)
        {
            var logs = await _itemActionLogService.GetLogsByClaimRequestIdAsync(claimRequestId);
            if (logs == null || !logs.Any())
            {
                return NotFound($"No action logs found for Claim Request ID: {claimRequestId}");
            }
            return Ok(logs);
        }
    }
}
