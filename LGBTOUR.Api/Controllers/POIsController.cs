using LGBTOUR.Api.Data;
using LGBTOUR.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POIsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public POIsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách tất cả địa điểm (GET)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetPOIs()
        {
            return await _context.POIs.ToListAsync();
        }

        // 2. Lấy chi tiết 1 địa điểm theo ID (GET)
        [HttpGet("{id}")]
        public async Task<ActionResult<POI>> GetPOI(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();
            return poi;
        }

        // 3. Thêm địa điểm mới (POST)
        [HttpPost]
        public async Task<ActionResult<POI>> CreatePOI(POI poi)
        {
            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPOI), new { id = poi.Id }, poi);
        }

        // 4. Sửa thông tin địa điểm (PUT)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePOI(int id, POI poi)
        {
            if (id != poi.Id) return BadRequest("ID không khớp!");

            _context.Entry(poi).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 5. Xóa địa điểm (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePOI(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound("Không tìm thấy địa điểm để xóa!");

            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}