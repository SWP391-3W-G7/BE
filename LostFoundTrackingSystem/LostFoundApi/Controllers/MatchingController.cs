using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.DTOs.MatchDTO;
using BLL.DTOs.Paging; // Added to access ItemMatchDto

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
        public async Task<ActionResult<IEnumerable<ItemMatchDto>>> GetMatchesForFoundItem(int foundItemId)
        {
            var matches = await _matchingService.GetMatchesForFoundItemAsync(foundItemId);
            return Ok(matches);
        }

        [HttpGet("lost-item/{lostItemId}")]
        public async Task<ActionResult<IEnumerable<ItemMatchDto>>> GetMatchesForLostItem(int lostItemId)
        {
            var matches = await _matchingService.GetMatchesForLostItemAsync(lostItemId);
            return Ok(matches);
        }

        [HttpGet("{matchId}")]
        public async Task<ActionResult<ItemMatchDto>> GetMatchDetails(int matchId)
        {
            var match = await _matchingService.GetMatchDetailsByIdAsync(matchId);
            if (match == null) return NotFound();
            return Ok(match);
        }

        [HttpPut("{matchId}/confirm")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ConfirmMatch(int matchId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffUserId = int.Parse(userIdClaim.Value);

            var result = await _matchingService.ConfirmMatchAsync(matchId, staffUserId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result); // Or BadRequest, depending on specific error handling
            }
        }

        [HttpPut("{matchId}/dismiss")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DismissMatch(int matchId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffUserId = int.Parse(userIdClaim.Value);

            var result = await _matchingService.DismissMatchAsync(matchId, staffUserId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result); // Or BadRequest, depending on specific error handling
            }
        }

        [HttpPut("{matchId}/conflict")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> ConflictMatch(int matchId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffUserId = int.Parse(userIdClaim.Value);

            try
            {
                await _matchingService.ConflictMatchAsync(matchId, staffUserId);
                return Ok("Match marked as conflicted.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetAllMatches([FromQuery] PagingParameters pagingParameters)
        {
            try
            {
                var result = await _matchingService.GetAllMatchesPagingAsync(pagingParameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //[Authorize]
        //public async Task<IActionResult> GetMyMatches([FromQuery] PagingParameters pagingParameters)
        //{
        //    try
        //    {
        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //        if (userIdClaim == null) return Unauthorized();
        //        int userId = int.Parse(userIdClaim.Value);

        //        var result = await _matchingService.GetMyMatchesPagingAsync(userId, pagingParameters);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}
