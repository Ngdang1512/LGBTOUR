using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SaigonAudioTour.AdminWeb.Options;

namespace SaigonAudioTour.AdminWeb.Controllers;

[Authorize]
public class ActivityController : Controller
{
    private readonly RealtimeOptions _realtimeOptions;

    public ActivityController(IOptions<RealtimeOptions> realtimeOptions)
    {
        _realtimeOptions = realtimeOptions.Value;
    }

    public IActionResult Index()
    {
        ViewData["ActivityHubUrl"] = _realtimeOptions.HubUrl;
        return View();
    }
}
