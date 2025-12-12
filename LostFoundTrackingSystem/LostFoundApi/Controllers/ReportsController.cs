using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service)
        {
            _service = service;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null) return Unauthorized("Role info missing.");

                if (!int.TryParse(roleClaim.Value, out int roleId))
                {
                    return Unauthorized("Invalid Role format.");
                }

                int? campusId = null;
                var campusClaim = User.FindFirst("CampusId");
                if (campusClaim != null && int.TryParse(campusClaim.Value, out int cId))
                {
                    campusId = cId;
                }

                var report = await _service.GetDashboardReportAsync(roleId, campusId);

                return Ok(report);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}