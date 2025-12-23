using BLL.DTOs;
using BLL.DTOs.AdminDTO;
using BLL.IServices;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize] 
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;
        private readonly IUserService _userService;
        private readonly IFoundItemService _foundItemService;
        private readonly ILostItemService _lostItemService;
        private readonly IClaimRequestService _claimService;

        public AdminController(IAdminService service, IUserService userService, IFoundItemService foundItemService, ILostItemService lostItemService, IClaimRequestService claimService)
        {
            _service = service;
            _userService = userService;
            _foundItemService = foundItemService;
            _lostItemService = lostItemService;
            _claimService = claimService;
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim != null && roleClaim.Value == "Admin";
        }

        [HttpPost("campuses")]
        public async Task<IActionResult> CreateCampus([FromBody] CreateCampusRequest request)
        {
            if (!IsAdmin()) return Forbid();

            var result = await _service.CreateCampusAsync(request);
            return Ok(result);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                await _service.AssignRoleAndCampusAsync(request);
                return Ok(new { message = "User role and campus updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("get-users-by-role")]
        public async Task<IActionResult> GetUsersByRole([FromQuery] int? roleId)
        {
            if(!IsAdmin()) return Forbid();
            try
            {
                var result = await _userService.GetUsersByRoleAsync(roleId);

                if (result == null || result.Count == 0)
                {
                    return NotFound("No users found with the specified role.");
                }

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserDto request)
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _userService.CreateUserByAdminAsync(request);
                return Ok(new
                {
                    message = "User created successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (!IsAdmin()) return Forbid();

            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound("User not found.");

            if (user.RoleId == 4) return Forbid();

            return Ok(user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto request)
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _userService.UpdateUserByAdminAsync(id, request);
                return Ok(new
                {
                    message = "User updated successfully.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPatch("users/{id}/ban-status")]
        public async Task<IActionResult> ChangeBanStatus(int id, [FromQuery] bool isBan)
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                await _userService.UpdateUserBanStatusAsync(id, isBan);

                string statusMessage = isBan ? "banned" : "unbanned/active";
                return Ok(new { message = $"User has been {statusMessage} successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("users/pending")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetPendingUsers()
        {
            try
            {
                var users = await _userService.GetPendingUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("users/{id}/approve")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            try
            {
                var loginResponse = await _userService.ApproveUserAsync(id);
                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("users/{id}/reject")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> RejectUser(int id)
        {
            try
            {
                await _userService.RejectUserAsync(id);
                return Ok(new { message = "User rejected successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //lay so luong do vat chua duoc tra tren he thong   
        [HttpGet("dashboard/unreturned-items-count")]
        public async Task<IActionResult> GetTotalUnreturnedItems()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var count = await _foundItemService.GetUnreturnedCountAsync(null);

                return Ok(new
                {
                    scope = "All Campuses",
                    count = count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //Lay thong ke do vat duoc tim thay theo thang trong nam
        [HttpGet("dashboard/found-items-monthly")]
        public async Task<IActionResult> GetMonthlyFoundItems([FromQuery] int? year)
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                int targetYear = year ?? DateTime.Now.Year;

                var stats = await _foundItemService.GetMonthlyStatsAsync(null, targetYear);

                return Ok(new
                {
                    year = targetYear,
                    scope = "All Campuses",
                    data = stats 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //Lay thong ke nguoi dung tim duoc nhieu do vat nhat
        [HttpGet("dashboard/top-contributor")]
        public async Task<IActionResult> GetTopContributorSystemWide()
        {
            if (!IsAdmin()) return Forbid();
            try
            {
                var result = await _foundItemService.GetTopContributorAsync(null);

                if (result == null) return Ok(new { message = "No data available yet." });

                return Ok(new { scope = "All Campuses", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //Lay thong ke campus co so luong do vat bi mat nhieu nhat
        [HttpGet("dashboard/campus-most-lost-items")]
        public async Task<IActionResult> GetCampusWithMostLostItems()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _lostItemService.GetCampusWithMostLostItemsAsync();

                if (result == null)
                {
                    return Ok(new { message = "No lost items recorded yet." });
                }

                return Ok(new
                {
                    message = "Campus with the highest number of lost items found.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //Lay thong ke nguoi dung bi mat do vat nhieu nhat
        [HttpGet("dashboard/user-most-lost-items")]
        public async Task<IActionResult> GetUserWithMostLostItems()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _lostItemService.GetTopLostItemUserAsync(null);

                if (result == null)
                {
                    return Ok(new { message = "No lost items data available." });
                }

                return Ok(new
                {
                    scope = "All Campuses",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //Lay thong ke trang thai do vat bi mat tren toan he thong
        [HttpGet("dashboard/lost-items-status-stats")]
        public async Task<IActionResult> GetLostItemsStatsSystemWide()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _lostItemService.GetLostItemStatisticsAsync(null);
                return Ok(new
                {
                    scope = "All Campuses",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //Lay thong ke trang thai do vat duoc tim thay tren toan he thong
        [HttpGet("dashboard/found-items-status-stats")]
        public async Task<IActionResult> GetFoundItemsStatsSystemWide()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                // Null = Toàn bộ hệ thống
                var result = await _foundItemService.GetFoundItemStatisticsAsync(null);
                return Ok(new
                {
                    scope = "All Campuses",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //Lay thong ke trang thai yeu cau nhan do vat tren toan he thong
        [HttpGet("dashboard/claim-status-stats")]
        public async Task<IActionResult> GetClaimStatsSystemWide()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _claimService.GetClaimStatisticsAsync(null);
                return Ok(new
                {
                    scope = "All Campuses",
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