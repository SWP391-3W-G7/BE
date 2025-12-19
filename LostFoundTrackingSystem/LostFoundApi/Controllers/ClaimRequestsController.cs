using BLL.DTOs.ClaimRequestDTO;
using BLL.DTOs.Paging;
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
            if (result.StudentId != currentUserId && !(User.IsInRole("Admin") || User.IsInRole("Staff"))) return Forbid();

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Staff,Admin")] // 2=Staff, 4=Admin
        public async Task<IActionResult> GetAll([FromQuery] ClaimStatus? status, [FromQuery] PagingParameters pagingParameters)
        {
            return Ok(await _service.GetAllPagingAsync(status, pagingParameters));
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
        [Authorize(Roles = "User,Staff,Security Officer")] // Only the student who owns the claim or Security Officer
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
        [HttpPatch("{id}/priority")]
        [Authorize(Roles = "Admin,Staff")] 
        public async Task<IActionResult> UpdatePriority(int id, [FromQuery] ClaimPriority priority)
        {
            try
            {
                await _service.UpdatePriorityAsync(id, priority);
                return Ok(new { message = $"Priority updated to {priority}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // PUT: api/claim-requests/{id}/request-evidence
        [HttpPut("{id}/request-evidence")]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Staff hoặc Admin mới được yêu cầu thêm bằng chứng
        public async Task<IActionResult> RequestMoreEvidence(int id, [FromBody] RequestMoreEvidenceDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffId = int.Parse(userIdClaim.Value);

            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "Message is required." });
            }

            try
            {
                await _service.RequestMoreEvidenceAsync(id, request.Message, staffId);

                return Ok(new { message = "Request for more evidence sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("staff/by-status")]
        [Authorize(Roles = "Staff,Admin,Security Officer")] // Staff hoặc Admin/Security đều xem được
        public async Task<IActionResult> GetClaimsByStatusForStaff([FromQuery] ClaimStatus status, [FromQuery] PagingParameters pagingParameters)
        {
            try
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null) return Unauthorized();

                var campusClaim = User.FindFirst("CampusId");
                if (campusClaim == null || !int.TryParse(campusClaim.Value, out int userCampusId))
                {
                    return BadRequest(new { message = "User does not have a valid Campus assigned." });
                }

                var result = await _service.GetClaimsByCampusAndStatusPagingAsync(userCampusId, status, pagingParameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}/approve")]
        /[Authorize(Roles = "Staff, Admin")] 
        public async Task<IActionResult> ApproveClaim(int id, [FromBody] ApproveClaimRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int staffId = int.Parse(userIdClaim.Value);

            try
            {
                var result = await _service.ApproveClaimAsync(id, request, staffId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}