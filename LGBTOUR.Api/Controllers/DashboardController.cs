using LGBTOUR.Api.DTOs.UserLogs;
using LGBTOUR.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUserLogService _userLogService;

        public DashboardController(IUserLogService userLogService)
        {
            _userLogService = userLogService;
        }

        // GET: api/dashboard/top-pois
        [HttpGet("top-pois")]
        public async Task<ActionResult<IEnumerable<PoiStatisticDto>>> GetTopPois([FromQuery] int top = 5)
        {
            var result = await _userLogService.GetTopListenedPoisAsync(top);
            return Ok(result);
        }

        // POST: api/dashboard/record-listen
        // (API này dành cho App Mobile gọi khi du khách nghe xong 1 bài thuyết minh)
        [HttpPost("record-listen")]
        public async Task<IActionResult> RecordListen([FromQuery] string userId, [FromQuery] int poiId, [FromQuery] int duration)
        {
            await _userLogService.RecordListenEventAsync(userId, poiId, duration);
            return Ok();
        }
    }
}