using BLL.DTOs.LostItemDTO;
using BLL.IServices;
using Microsoft.AspNetCore.Mvc;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/lost-items")]
    public class LostItemsController : ControllerBase
    {
        private readonly ILostItemService _service;
        public LostItemsController(ILostItemService service)
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
        public async Task<IActionResult> Create([FromForm] CreateLostItemRequest request)
        {
            var createdItem = await _service.CreateAsync(request);
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
    }
}
