using SaigonAudioTour.Api.DTOs.UserLogs;
using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Controllers
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PoiStatisticDto>>> GetTopPois([FromQuery] int top = 5)
        {
            var result = await _userLogService.GetTopListenedPoisAsync(top);
            return Ok(result);
        }

        // POST: api/dashboard/record-listen
        [HttpPost("record-listen")]
        [AllowAnonymous] // MỞ CỬA cho Mobile App gọi ngầm không cần đăng nhập
        public async Task<IActionResult> RecordListen([FromQuery] string userId, [FromQuery] int poiId, [FromQuery] int duration)
        {
            await _userLogService.RecordListenEventAsync(userId, poiId, duration);
            return Ok();
        }
    }
}