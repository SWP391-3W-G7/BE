using BLL.DTOs.Paging;
using BLL.DTOs.StaffDTO;
using BLL.IServices;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/staff")]
    [Authorize(Roles = "Staff")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        private readonly IFoundItemService _foundItemService;
        private readonly ILostItemService _lostItemService;
        private readonly IClaimRequestService _claimService;

        public StaffController(IStaffService staffService, IFoundItemService foundItemService, ILostItemService lostItemService, IClaimRequestService claimService)
        {
            _staffService = staffService;
            _foundItemService = foundItemService;
            _lostItemService = lostItemService;
            _claimService = claimService;
        }

        [HttpGet("work-items")]
        public async Task<IActionResult> GetWorkItems([FromQuery] PagingParameters pagingParameters)
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null)
            {
                return Unauthorized("User is not associated with a campus.");
            }
            var campusId = int.Parse(campusIdClaim.Value);
            var workItems = await _staffService.GetWorkItemsAsync(campusId, pagingParameters);
            return Ok(workItems);
        }
        [HttpPost("found-items/{id}/request-dropoff")]
        public async Task<IActionResult> RequestDropOff(int id, [FromBody] RequestDropOffDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffId = int.Parse(userIdClaim.Value);

            try
            {
                await _staffService.RequestItemDropOffAsync(id, request, staffId);
                return Ok(new { message = "Drop-off request sent to student successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay so luong do vat chua tra ve tren campus cua staff dang nhap
        [HttpGet("dashboard/unreturned-items-count")]
        public async Task<IActionResult> GetMyCampusUnreturnedItems()
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null)
            {
                return Unauthorized("Staff user is not associated with a campus.");
            }
            int campusId = int.Parse(campusIdClaim.Value);

            try
            {
                var count = await _foundItemService.GetUnreturnedCountAsync(campusId);

                return Ok(new
                {
                    scope = $"Campus ID {campusId}",
                    count = count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay thong ke do vat tim thay theo thang tren campus cua staff dang nhap
        [HttpGet("dashboard/found-items-monthly")]
        public async Task<IActionResult> GetMyCampusMonthlyFoundItems([FromQuery] int? year)
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null) return Unauthorized("Staff has no CampusId");
            int campusId = int.Parse(campusIdClaim.Value);

            try
            {
                int targetYear = year ?? DateTime.Now.Year;

                var stats = await _foundItemService.GetMonthlyStatsAsync(campusId, targetYear);

                return Ok(new
                {
                    year = targetYear,
                    scope = $"Campus ID {campusId}",
                    data = stats
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay thong tin nguoi tim duoc nhieu do vat nhat tren campus cua staff dang nhap
        [HttpGet("dashboard/top-contributor")]
        public async Task<IActionResult> GetTopContributorMyCampus()
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null) return Unauthorized();
            int campusId = int.Parse(campusIdClaim.Value);

            try
            {
                var result = await _foundItemService.GetTopContributorAsync(campusId);

                if (result == null) return Ok(new { message = "No data available yet." });

                return Ok(new { scope = $"Campus {campusId}", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay thong tin nguoi mat nhieu do vat nhat tren campus cua staff dang nhap
        [HttpGet("dashboard/user-most-lost-items")]
        public async Task<IActionResult> GetMyCampusUserWithMostLostItems()
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null) return Unauthorized("Staff has no CampusId");
            int campusId = int.Parse(campusIdClaim.Value);

            try
            {
                var result = await _lostItemService.GetTopLostItemUserAsync(campusId);

                if (result == null)
                {
                    return Ok(new { message = "No lost items data available for this campus." });
                }

                return Ok(new
                {
                    scope = $"Campus ID {campusId}",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay thong ke trang thai do vat mat tren campus cua staff dang nhap
        [HttpGet("dashboard/lost-items-status-stats")]
        public async Task<IActionResult> GetMyCampusLostItemsStats()
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null) return Unauthorized("Staff has no CampusId");
            int campusId = int.Parse(campusIdClaim.Value);

            try
            {
                var result = await _lostItemService.GetLostItemStatisticsAsync(campusId);
                return Ok(new
                {
                    scope = $"Campus ID {campusId}",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay thong ke trang thai do vat tim thay tren campus cua staff dang nhap
        [HttpGet("dashboard/found-items-status-stats")]
        public async Task<IActionResult> GetMyCampusFoundItemsStats()
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null) return Unauthorized("Staff has no CampusId");
            int campusId = int.Parse(campusIdClaim.Value);

            try
            {
                var result = await _foundItemService.GetFoundItemStatisticsAsync(campusId);
                return Ok(new
                {
                    scope = $"Campus ID {campusId}",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay thong ke trang thai yeu cau nhan do vat tren campus cua staff dang nhap
        [HttpGet("dashboard/claim-status-stats")]
        public async Task<IActionResult> GetMyCampusClaimStats()
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null) return Unauthorized("Staff has no CampusId");
            int campusId = int.Parse(campusIdClaim.Value);

            try
            {
                var result = await _claimService.GetClaimStatisticsAsync(campusId);
                return Ok(new
                {
                    scope = $"Campus ID {campusId}",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
