using BLL.DTOs.ClaimRequestDTO;
using BLL.IServices;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LostFoundApi.Controllers
{
    [ApiController]
    [Route("api/claim-requests")]
    [Authorize]
    public class ClaimRequestsController : ControllerBase
    {
        private readonly IClaimRequestService _service;

        public ClaimRequestsController(IClaimRequestService service)
        {
            _service = service;
        }

        // POST: api/claim-requests
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateClaimRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int studentId = int.Parse(userIdClaim.Value);

            var result = await _service.CreateAsync(request, studentId);
            return Ok(result);
        }

        [HttpGet("my-claims")]
        public async Task<IActionResult> GetMyClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int studentId = int.Parse(userIdClaim.Value);

            var result = await _service.GetMyClaimsAsync(studentId);
            return Ok(result);
        }

        // GET: api/claim-requests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int currentUserId = int.Parse(userIdClaim.Value);
            if (result.StudentId != currentUserId && !User.IsInRole("Admin, Staff")) return Forbid();

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")] // 2=Staff, 4=Admin
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateClaimRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int studentId = int.Parse(userIdClaim.Value);

            try
            {
                var result = await _service.UpdateAsync(id, request, studentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Trả về lỗi nếu không phải chủ sở hữu hoặc status != Pending
            }
        }

        // PATCH: api/claim-requests/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")] // 4=Admin, 2=Staff
        public async Task<IActionResult> ChangeStatus(int id, [FromQuery] ClaimStatus status)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffId = int.Parse(userIdClaim.Value);

            try
            {
                var result = await _service.UpdateStatusAsync(id, status, staffId);
                return Ok(new
                {
                    result,
                    Message = $"Claim request status updated to {status}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{claimId}/conflict")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ConflictClaim(int claimId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffId = int.Parse(userIdClaim.Value);

            try
            {
                await _service.ConflictClaimAsync(claimId, staffId);
                return Ok("Claim request marked as conflicted.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{claimId}/evidence")]
        [Authorize(Roles = "User,Security Officer")] // Only the student who owns the claim or Security Officer
        public async Task<IActionResult> AddEvidence(int claimId, [FromForm] AddEvidenceRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            try
            {
                await _service.AddEvidenceToClaimAsync(claimId, request, userId);
                return Ok("Evidence added successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}