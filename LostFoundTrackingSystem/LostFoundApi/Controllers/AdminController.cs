using BLL.DTOs;
using BLL.DTOs.AdminDTO;
using BLL.IServices;
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

        public AdminController(IAdminService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
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
    }
}