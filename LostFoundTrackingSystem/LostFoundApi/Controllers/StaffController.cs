using BLL.IServices;
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

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet("work-items")]
        public async Task<IActionResult> GetWorkItems()
        {
            var campusIdClaim = User.FindFirst("CampusId");
            if (campusIdClaim == null)
            {
                return Unauthorized("User is not associated with a campus.");
            }
            var campusId = int.Parse(campusIdClaim.Value);
            var workItems = await _staffService.GetWorkItemsAsync(campusId);
            return Ok(workItems);
        }
    }
}
