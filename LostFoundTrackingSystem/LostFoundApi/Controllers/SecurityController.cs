using BLL.DTOs.FoundItemDTO;
using BLL.IServices;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/security")]
    [Authorize(Roles = "Security Officer")]
    public class SecurityController : ControllerBase
    {
        private readonly IFoundItemService _foundItemService;
        private readonly IUserService _userService;

        public SecurityController(IFoundItemService foundItemService, IUserService userService)
        {
            _foundItemService = foundItemService;
            _userService = userService;
        }

        [HttpGet("my-open-found-items")]
        public async Task<IActionResult> GetMyOpenFoundItems()
        {
            var campusClaim = User.FindFirst("CampusId");
            if (campusClaim == null) return Unauthorized("Campus info missing.");
            if (!int.TryParse(campusClaim.Value, out int campusId))
            {
                return Unauthorized("Invalid CampusId format.");
            }

            var items = await _foundItemService.GetOpenFoundItemsForSecurityOfficerAsync(campusId);
            return Ok(items);
        }

        [HttpPut("found-items/{id}/return")]
        public async Task<IActionResult> ReturnFoundItem(int id)
        {
            try
            {
                var securityOfficerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var request = new UpdateFoundItemStatusRequest { Status = FoundItemStatus.Returned.ToString() };
                var result = await _foundItemService.UpdateStatusAsync(id, request, securityOfficerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
