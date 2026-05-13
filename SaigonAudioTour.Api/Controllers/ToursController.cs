using SaigonAudioTour.Api.DTOs.Tours;
using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        private readonly ITourService _tourService;

        public ToursController(ITourService tourService)
        {
            _tourService = tourService;
        }

        // --- DÀNH CHO KHÁCH DU LỊCH (MOBILE APP) ---

        // GET: api/tours
        [HttpGet]
        [AllowAnonymous] // THÊM MỚI: Mobile App cần lấy danh sách các tuyến để chọn
        public async Task<ActionResult<IEnumerable<TourDetailDto>>> GetAllTours()
        {
            var tours = await _tourService.GetAllToursAsync();
            return Ok(tours);
        }

        // GET: api/tours/5
        [HttpGet("{id}")]
        [AllowAnonymous] // MỞ CỬA: Khách phải xem được chi tiết tuyến xe chứ!
        public async Task<ActionResult<TourDetailDto>> GetTour(int id)
        {
            var tour = await _tourService.GetTourByIdAsync(id);
            if (tour == null) return NotFound(new { message = "Không tìm thấy Tuyến xe buýt này." });

            return Ok(tour);
        }


        // --- DÀNH CHO ADMIN CMS ---

        // POST: api/tours
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TourDetailDto>> CreateTour([FromBody] CreateTourDto dto)
        {
            var newTour = await _tourService.CreateTourAsync(dto);
            return CreatedAtAction(nameof(GetTour), new { id = newTour.Id }, newTour);
        }

        // POST: api/tours/5/pois
        [HttpPost("{tourId}/pois")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPoiToTour(int tourId, [FromBody] AddPoiToTourDto dto)
        {
            var success = await _tourService.AddPoiToTourAsync(tourId, dto);
            if (!success) return BadRequest(new { message = "Lỗi! Tuyến xe buýt hoặc Trạm đến không tồn tại." });

            return Ok(new { message = "Đã thêm trạm vào Tuyến xe buýt thành công!" });
        }

        // PUT: api/tours/{id}/route
        [HttpPut("{id}/route")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] List<int> orderedPoiIds)
        {
            var success = await _tourService.UpdateRouteAsync(id, orderedPoiIds);
            if (!success) return NotFound(new { message = "Không tìm thấy Tuyến xe buýt này." });

            return Ok(new { message = "Đã lưu lộ trình tuyến xe thành công!" });
        }

        // PUT: api/tours/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTour(int id, [FromBody] UpdateTourDto dto)
        {
            var success = await _tourService.UpdateTourAsync(id, dto);
            if (!success) return NotFound(new { message = "Không tìm thấy Tuyến xe buýt này." });

            return Ok(new { message = "Đã cập nhật thông tin Tuyến xe buýt thành công!" });
        }

        // DELETE: api/tours/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var success = await _tourService.DeleteTourAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy Tuyến xe buýt này để xóa." });

            return Ok(new { message = "Đã xóa Tuyến xe buýt thành công!" });
        }
    }
}