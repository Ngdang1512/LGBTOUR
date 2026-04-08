using LGBTOUR.Api.DTOs.Narrations;
using LGBTOUR.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NarrationsController : ControllerBase
    {
        private readonly INarrationService _narrationService;

        public NarrationsController(INarrationService narrationService)
        {
            _narrationService = narrationService;
        }
        [Authorize]
        // POST: api/narrations
        [HttpPost]
        [Consumes("multipart/form-data")] // Ép buộc API này chỉ nhận Form-Data (để upload file)
        public async Task<IActionResult> CreateNarration([FromForm] CreateNarrationDto dto)
        {
            var result = await _narrationService.AddNarrationAsync(dto);

            if (result == null)
            {
                return NotFound("Không tìm thấy Quán ăn (POI) tương ứng để thêm thuyết minh.");
            }

            return Ok(result);
        }
    }
}