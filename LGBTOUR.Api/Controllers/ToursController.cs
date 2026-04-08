using LGBTOUR.Api.DTOs.Tours;
using LGBTOUR.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Controllers
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
        [Authorize]
        // GET: api/tours/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TourDetailDto>> GetTour(int id)
        {
            var tour = await _tourService.GetTourByIdAsync(id);
            if (tour == null) return NotFound("Không tìm thấy Tour này.");

            return Ok(tour);
        }
        [Authorize]
        // POST: api/tours
        [HttpPost]
        public async Task<ActionResult<TourDetailDto>> CreateTour([FromBody] CreateTourDto dto)
        {
            var newTour = await _tourService.CreateTourAsync(dto);
            return CreatedAtAction(nameof(GetTour), new { id = newTour.Id }, newTour);
        }
        [Authorize]
        // POST: api/tours/5/pois
        // API này dùng để "nhét" 1 quán ăn vào 1 tour cụ thể
        [HttpPost("{tourId}/pois")]
        public async Task<IActionResult> AddPoiToTour(int tourId, [FromBody] AddPoiToTourDto dto)
        {
            var success = await _tourService.AddPoiToTourAsync(tourId, dto);
            if (!success) return BadRequest("Lỗi! Tour hoặc Điểm đến không tồn tại.");

            return Ok("Đã thêm điểm đến vào Tour thành công!");
        }
    }
}