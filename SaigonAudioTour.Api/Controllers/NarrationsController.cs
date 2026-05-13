using SaigonAudioTour.Api.DTOs.Narrations;
using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Controllers
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

        // GET: api/narrations/{poiId}?lang=vi
        [HttpGet("{poiId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNarrationByPoi(int poiId, [FromQuery] string? lang = "vi")
        {
            var result = await _narrationService.GetNarrationByPoiAndLanguageAsync(poiId, lang);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy nội dung thuyết minh cho địa điểm này." });
            }

            return Ok(result);
        }

        // POST: api/narrations
        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        // GET: api/narrations/tts/{poiId}?lang=vi
        [HttpGet("tts/{poiId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateTtsFallback(int poiId, [FromQuery] string? lang = "vi")
        {
            var result = await _narrationService.GenerateTtsFallbackAsync(poiId, lang);
            if (result == null)
            {
                return NotFound(new { message = "Không thể tạo TTS cho địa điểm này." });
            }

            return Ok(result);
        }

        // GET: api/narrations/{poiId}/synthesize?lang=vi
        [HttpGet("{poiId:int}/synthesize")]
        [AllowAnonymous]
        public async Task<IActionResult> SynthesizeNarration(int poiId, [FromQuery] string? lang = "vi")
        {
            var result = await _narrationService.GenerateTtsFallbackAsync(poiId, lang);
            if (result == null)
            {
                return NotFound(new { message = "Không thể tạo audio cho địa điểm này." });
            }

            return Ok(result);
        }
    }
}