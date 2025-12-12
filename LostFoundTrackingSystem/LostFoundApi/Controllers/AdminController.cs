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

        public AdminController(IAdminService service)
        {
            _service = service;
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
    }
}