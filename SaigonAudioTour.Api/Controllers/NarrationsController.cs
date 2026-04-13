using SaigonAudioTour.Api.DTOs.Narrations;
using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class NarrationsController : ControllerBase
    {
        private readonly INarrationService _narrationService;

        public NarrationsController(INarrationService narrationService)
        {
            _narrationService = narrationService;
        }

        // POST: api/narrations
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateNarration([FromForm] CreateNarrationDto dto)
        {
            var result = await _narrationService.AddNarrationAsync(dto);

            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy Quán ăn/Địa danh (POI) tương ứng để thêm thuyết minh." });
            }

            return Ok(result);
        }
    }
}