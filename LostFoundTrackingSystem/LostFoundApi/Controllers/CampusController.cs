using BLL.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DAL.Models;
using BLL.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using BLL.DTOs.CampusDTO;
using Microsoft.AspNetCore.Authorization;

namespace LostFoundApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly ICampusService _campusService;

        public CampusController(ICampusService campusService)
        {
            _campusService = campusService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var campuses = await _campusService.GetAllAsync();
            return Ok(campuses);
        }

        [HttpGet("enum-values")]
        public IActionResult GetCampusEnumValues()
        {
            var enumValues = Enum.GetValues(typeof(CampusEnum))
                               .Cast<CampusEnum>()
                               .Select(e => new
                               {
                                   Id = (int)e,
                                   Name = e.ToString(),
                                   Description = e.GetDescription()
                               })
                               .ToList();
            return Ok(enumValues);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var campus = await _campusService.GetByIdAsync(id);
            if (campus == null) return NotFound();
            return Ok(campus);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCampusDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdCampus = await _campusService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdCampus.CampusId }, createdCampus);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCampusDto request)
        {
            try
            {
                await _campusService.UpdateAsync(id, request);
                return Ok(new { message = "Campus updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _campusService.DeleteAsync(id);
                return Ok(new { message = "Campus deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
