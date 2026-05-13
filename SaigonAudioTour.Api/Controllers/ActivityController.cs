using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaigonAudioTour.Api.Models.Realtime;
using SaigonAudioTour.Api.Services;

namespace SaigonAudioTour.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ActivityController : ControllerBase
{
    private readonly IUserActivityStore _store;

    public ActivityController(IUserActivityStore store)
    {
        _store = store;
    }

    [HttpGet("online")]
    [Authorize(Roles = "Admin")]
    public ActionResult<OnlineActivitySnapshotDto> GetOnlineUsers()
    {
        return Ok(new OnlineActivitySnapshotDto
        {
            OnlineCount = _store.OnlineCount,
            Users = _store.GetSnapshot().ToList()
        });
    }
}
