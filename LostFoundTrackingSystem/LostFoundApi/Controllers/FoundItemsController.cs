using BLL.DTOs.FoundItemDTO;
using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DAL.Models; // Added to access FoundItemStatus enum

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/found-items")]
    public class FoundItemsController : ControllerBase
    {
        private readonly IFoundItemService _foundItemService;
        private readonly IUserService _userService;

        public FoundItemsController(IFoundItemService foundItemService, IUserService userService)
        {
            _foundItemService = foundItemService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _foundItemService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost("staff")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> CreateFoundItemByStaff([FromForm] CreateFoundItemRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                string initialStatus = FoundItemStatus.Stored.ToString();

                var result = await _foundItemService.CreateAsync(request, userId, initialStatus);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("public")]
        [Authorize(Roles = "User,Security Officer")] // User = Student role
        public async Task<IActionResult> CreateFoundItemByStudentOrSecurityOfficer([FromForm] CreateFoundItemRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                string initialStatus = FoundItemStatus.Open.ToString();

                var result = await _foundItemService.CreateAsync(request, userId, initialStatus);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Staff,Security Officer")]
        public async Task<IActionResult> UpdateFoundItemStatus(int id, [FromBody] UpdateFoundItemStatusRequest request)
        {
            try
            {
                var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _foundItemService.UpdateStatusAsync(id, request, staffId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("campus")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetFoundItemsByCampus()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _userService.GetByIdAsync(userId);
                if (user?.CampusId == null)
                {
                    return BadRequest("User is not associated with a campus.");
                }

                var result = await _foundItemService.GetByCampusAsync(user.CampusId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("campus/open")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetOpenFoundItemsByCampus()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _userService.GetByIdAsync(userId);
                if (user?.CampusId == null)
                {
                    return BadRequest("User is not associated with a campus.");
                }

                var result = await _foundItemService.GetByCampusAsync(user.CampusId.Value, FoundItemStatus.Open.ToString());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/details")]
        [Authorize(Roles = "Staff,Admin")] // 2=Staff, 4=Admin
        public async Task<IActionResult> GetFoundItemDetails(int id)
        {
            try
            {
                var result = await _foundItemService.GetFoundItemDetailsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/user-details")]
        [Authorize(Roles = "User,Security Officer,Staff,Admin")] // Accessible to all authenticated users
        public async Task<IActionResult> GetFoundItemDetailsForUser(int id)
        {
            try
            {
                var result = await _foundItemService.GetFoundItemDetailsForUserAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}