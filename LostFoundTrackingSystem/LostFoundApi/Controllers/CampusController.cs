using BLL.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
    }
}
