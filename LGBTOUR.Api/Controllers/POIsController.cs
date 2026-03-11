using LGBTOUR.Api.Entities;
using LGBTOUR.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POIsController : ControllerBase
    {
        private readonly IPOIRepository _poiRepository;

        // Inject Repository vào để dùng
        public POIsController(IPOIRepository poiRepository)
        {
            _poiRepository = poiRepository;
        }

        // GET: api/POIs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetPOIs()
        {
            var pois = await _poiRepository.GetAllAsync();
            return Ok(pois);
        }

        // GET: api/POIs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<POI>> GetPOI(int id)
        {
            var poi = await _poiRepository.GetByIdAsync(id);
            if (poi == null) return NotFound();
            return Ok(poi);
        }

        // POST: api/POIs
        [HttpPost]
        public async Task<ActionResult<POI>> CreatePOI(POI poi)
        {
            await _poiRepository.AddAsync(poi);
            return CreatedAtAction(nameof(GetPOI), new { id = poi.Id }, poi);
        }
    }
}