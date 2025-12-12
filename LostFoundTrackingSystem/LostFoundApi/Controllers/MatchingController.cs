using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchingController : ControllerBase
    {
        private readonly IMatchingService _matchingService;

        public MatchingController(IMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        [HttpPost("lost-item/{lostItemId}/find-matches")]
        public async Task<IActionResult> FindMatchesForLostItem(int lostItemId)
        {
            try
            {
                await _matchingService.FindAndCreateMatchesAsync(lostItemId);
                return Ok("Matching process completed.");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("found-item/{foundItemId}")]
        public async Task<IActionResult> GetMatchesForFoundItem(int foundItemId)
        {
            var matches = await _matchingService.GetMatchesForFoundItemAsync(foundItemId);
            return Ok(matches);
        }

        [HttpPut("{matchId}/confirm")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ConfirmMatch(int matchId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffUserId = int.Parse(userIdClaim.Value);

            await _matchingService.ConfirmMatchAsync(matchId, staffUserId);
            return Ok();
        }

        [HttpPut("{matchId}/dismiss")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DismissMatch(int matchId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffUserId = int.Parse(userIdClaim.Value);

            await _matchingService.DismissMatchAsync(matchId, staffUserId);
            return Ok();
        }
    }
}
