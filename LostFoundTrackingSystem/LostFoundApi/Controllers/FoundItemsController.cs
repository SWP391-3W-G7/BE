using BLL.DTOs.FoundItemDTO;
using BLL.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/found-items")]
    public class FoundItemsController : ControllerBase
    {
        private readonly IFoundItemService _service;

        public FoundItemsController(IFoundItemService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateFoundItemRequest request)
        {
            var createdItem = await _service.CreateAsync(request);
            return Ok(createdItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateFoundItemRequest request)
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

        [HttpGet("campus/{campusId}")]
        public async Task<IActionResult> GetByCampus(int campusId)
        {
            var items = await _service.GetByCampusAsync(campusId);
            return Ok(items);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var items = await _service.GetByCategoryAsync(categoryId);
            return Ok(items);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByTitle([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Title search keyword is required.");
            }
            var items = await _service.SearchByTitleAsync(title);
            return Ok(items);
        }
    }
}