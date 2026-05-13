using SaigonAudioTour.Api.DTOs.Pois;
using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoisController : ControllerBase
    {
        private readonly IPoiService _poiService;

        public PoisController(IPoiService poiService)
        {
            _poiService = poiService;
        }

        // --- DÀNH CHO KHÁCH DU LỊCH (MOBILE APP) ---

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PoiDto>>> GetAllPois()
        {
            var result = await _poiService.GetAllPoisAsync();
            return Ok(result);
        }

        [HttpGet("nearby")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNearbyPoi([FromQuery] double lat, [FromQuery] double lng, [FromQuery] string lang = "vi")
        {
            var nearbyPoi = await _poiService.GetNearbyPoiAsync(lat, lng, lang);

            if (nearbyPoi == null)
            {
                return Ok(null);
            }

            return Ok(nearbyPoi);
        }


        // --- DÀNH CHO ADMIN CMS ---
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PoiDto>> CreatePoi([FromBody] CreatePoiDto createDto)
        {
            var createdPoi = await _poiService.CreatePoiAsync(createDto);

            // ĐÃ FIX LỖI TẠI ĐÂY: Dùng Ok() thay vì CreatedAtAction để tránh lỗi Routing
            return Ok(createdPoi);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePoi(int id, [FromBody] UpdatePoiDto dto)
        {
            var success = await _poiService.UpdatePoiAsync(id, dto);
            if (!success) return NotFound(new { message = "Không tìm thấy Trạm/Địa danh này." });

            return Ok(new { message = "Đã cập nhật thông tin thành công!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePoi(int id)
        {
            var success = await _poiService.DeletePoiAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy Trạm/Địa danh này để xóa." });

            return Ok(new { message = "Đã xóa Trạm/Địa danh thành công!" });
        }

        [HttpPost("{id}/image")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(int id, IFormFile imageFile)
        {
            var success = await _poiService.UploadImageAsync(id, imageFile);

            if (!success)
            {
                return BadRequest(new { message = "Không tìm thấy Trạm, hoặc file ảnh không hợp lệ." });
            }

            return Ok(new { message = "Đã upload và cập nhật hình ảnh thành công!" });
        }

        // --- SEED DEMO DATA (Admin Only) ---
        [HttpPost("seed-demo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedDemoData()
        {
            try
            {
                var demoCount = await _poiService.SeedDemoPoisAsync();
                return Ok(new { message = $"Đã tạo {demoCount} POI demo. Có thể reload webapp để xem!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi seed data: " + ex.Message });
            }
        }
    }
}