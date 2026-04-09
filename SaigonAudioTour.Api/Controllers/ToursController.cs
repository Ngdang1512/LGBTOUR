using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ToursController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetTours()
        {
            return await _context.Tours.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tour>> GetTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();
            return tour;
        }

        [HttpPost]
        public async Task<ActionResult<Tour>> CreateTour(Tour tour)
        {
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTour), new { id = tour.Id }, tour);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTour(int id, Tour tour)
        {
            if (id != tour.Id) return BadRequest("ID không khớp");
            _context.Entry(tour).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();
            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}