using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/found-items")]
    public class FoundItemsController : ControllerBase
    {
        private readonly IFoundItemService _foundItemService;

        public FoundItemsController(IFoundItemService foundItemService)
        {
            _foundItemService = foundItemService;
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
    }
}