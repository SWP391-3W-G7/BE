using BLL.DTOs.ReturnRecordDTO;
using BLL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/return-records")]
    [Authorize] // Cần bảo mật, tốt nhất là [Authorize(Roles = "Staff,Admin")]
    public class ReturnRecordsController : ControllerBase
    {
        private readonly IReturnRecordService _service;

        public ReturnRecordsController(IReturnRecordService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "2,4")]
        public async Task<IActionResult> Create([FromBody] CreateReturnRecordRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int staffId = int.Parse(userIdClaim.Value);

                var result = await _service.CreateReturnRecordAsync(request, staffId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "2,4")]
        public async Task<IActionResult> GetAll()
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int currentUserId = int.Parse(userIdClaim.Value);
            if (result.ReceiverId != currentUserId && !User.IsInRole("Admin, Staff")) return Forbid();

            return Ok(result);
        }
        [HttpGet("my-returns")]
        public async Task<IActionResult> GetMyReturns()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int receiverId = int.Parse(userIdClaim.Value);

            var results = await _service.GetMyRecord(receiverId);
            return Ok(results);
        }
    }
}