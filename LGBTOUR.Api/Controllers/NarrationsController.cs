using LGBTOUR.Api.DTOs.Narrations;
using LGBTOUR.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Đưa lên đây: KHÓA TOÀN BỘ CONTROLLER NÀY (Chỉ Admin mới được thao tác audio)
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