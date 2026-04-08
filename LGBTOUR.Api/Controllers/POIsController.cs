using LGBTOUR.Api.DTOs.Pois;
using LGBTOUR.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoisController : ControllerBase
    {
        private readonly IPoiService _poiService;

        // Tiêm (Inject) Service vào Controller
        public PoisController(IPoiService poiService)
        {
            _poiService = poiService;
        }
        [Authorize]
        // GET: api/pois
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PoiDto>>> GetAllPois()
        {
            var result = await _poiService.GetAllPoisAsync();
            return Ok(result); // Trả về HTTP Status 200 kèm data
        }
        [Authorize]
        // POST: api/pois
        [HttpPost]
        public async Task<ActionResult<PoiDto>> CreatePoi([FromBody] CreatePoiDto createDto)
        {
            // [ApiController] đã tự động kiểm tra [Required] trong DTO cho bạn rồi
            var createdPoi = await _poiService.CreatePoiAsync(createDto);

            // Trả về HTTP Status 201 (Created)
            return CreatedAtAction(nameof(GetAllPois), new { id = createdPoi.Id }, createdPoi);
        }
    }
}