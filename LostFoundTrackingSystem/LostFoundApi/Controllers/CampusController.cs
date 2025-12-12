using BLL.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DAL.Models;
using BLL.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;

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
    }
}
