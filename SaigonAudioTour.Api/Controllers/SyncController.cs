using SaigonAudioTour.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SyncController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Chức năng: Gói toàn bộ dữ liệu thành 1 cục để tải Offline
        [HttpGet("offline-data")]
        public async Task<IActionResult> GetOfflineData()
        {
            // Lấy toàn bộ Tour, KÈM THEO danh sách các điểm POI bên trong Tour đó
            var toursData = await _context.Tours
                .Include(t => t.TourPOIs)        // Móc vào bảng Cầu nối
                .ThenInclude(tp => tp.POI)       // Từ bảng cầu nối, lôi chi tiết Địa điểm ra
                .OrderBy(t => t.Id)
                .ToListAsync();

            // Đóng gói thành 1 cục JSON xịn sò trả về cho App Mobile
            return Ok(new
            {
                message = "Tải dữ liệu Offline thành công!",
                totalTours = toursData.Count,
                data = toursData
            });
        }
    }
}