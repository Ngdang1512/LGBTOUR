using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LGBTOUR.Api.Data;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileAppController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MobileAppController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API: LẤY DANH SÁCH CÁC TUYẾN XE (TOURS)
        // Mobile gọi API này để hiển thị danh sách Tour cho khách chọn mua/tham gia
        [HttpGet("tours")]
        public async Task<IActionResult> GetTours()
        {
            var tours = await _context.Tours
                .Select(t => new {
                    t.Id,
                    t.Name,
                    t.Description,
                    t.Price,
                    TotalStops = t.TourPOIs.Count() // Đếm luôn số trạm dừng cho app hiển thị
                })
                .ToListAsync();

            return Ok(new { success = true, data = tours });
        }

        // 2. API: LẤY LỘ TRÌNH VÀ AUDIO CỦA 1 TOUR CỤ THỂ
        // Mobile gọi API này khi bắt đầu lên xe. Cần truyền vào TourId và Ngôn ngữ (mặc định 'vi')
        [HttpGet("tours/{tourId}/pois")]
        public async Task<IActionResult> GetTourRoute(int tourId, string lang = "vi")
        {
            var route = await _context.TourPOIs
                .Include(tp => tp.POI)
                .ThenInclude(p => p.Audios)
                .Where(tp => tp.TourId == tourId)
                .OrderBy(tp => tp.DisplayOrder) // Sắp xếp đúng thứ tự xe chạy
                .Select(tp => new {
                    POI_Id = tp.POI.Id,
                    Name = tp.POI.Name,
                    Description = tp.POI.Description,
                    Lat = tp.POI.Lat,
                    Lng = tp.POI.Lng,
                    Radius = tp.POI.Radius, // Cực kỳ quan trọng để Mobile làm Geofencing

                    // Tự động tìm file Audio khớp với ngôn ngữ khách chọn
                    AudioUrl = tp.POI.Audios.FirstOrDefault(a => a.LanguageCode == lang) != null
                               ? tp.POI.Audios.FirstOrDefault(a => a.LanguageCode == lang).AudioUrl
                               : ""
                })
                .ToListAsync();

            if (!route.Any())
            {
                return NotFound(new { success = false, message = "Tour này chưa được thiết lập lộ trình!" });
            }

            return Ok(new { success = true, data = route });
        }
    }
}