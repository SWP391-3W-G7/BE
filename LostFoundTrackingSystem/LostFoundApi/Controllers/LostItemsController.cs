using System.Security.Claims;
using BLL.DTOs.LostItemDTO; // Added
using BLL.DTOs.Paging;
using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/lost-items")]
    [Authorize]
    public class LostItemsController : ControllerBase
    {
        private readonly ILostItemService _service;
        public LostItemsController(ILostItemService service)
        {
            _service = service;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] LostItemFilterDto filter, [FromQuery] PagingParameters pagingParameters)
        {
            return Ok(await _service.GetAllPagingAsync(filter, pagingParameters));
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateLostItemRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var createdBy = int.Parse(userId.Value);
            var createdItem = await _service.CreateAsync(request, createdBy);
            return Ok(createdItem);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateLostItemRequest request)
        {
            var updatedItem = await _service.UpdateAsync(id, request);
            return Ok(updatedItem);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("my-lost-items")]
        public async Task<IActionResult> GetMyLostItems()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);
            var lostItems = await _service.GetMyLostItemsAsync(userId);
            return Ok(lostItems);
        }

        [HttpGet("campus/{campusId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCampus(int campusId)
        {
            var items = await _service.GetByCampusAsync(campusId);
            return Ok(items);
        }
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var items = await _service.GetByCategoryAsync(categoryId);
            return Ok(items);
        }
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchByTitle([FromQuery] string title)
        {
            var items = await _service.SearchByTitleAsync(title);
            return Ok(items);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")] // 4=Admin, 2=Staff
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffId = int.Parse(userIdClaim.Value);

            try
            {
                var result = await _service.UpdateStatusAsync(id, new UpdateLostItemStatusRequest { Status = status }, staffId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
