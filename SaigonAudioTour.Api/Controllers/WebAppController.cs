using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaigonAudioTour.Api.Controllers;

[ApiController]
[AllowAnonymous]
public class WebAppController : ControllerBase
{
    [HttpGet("qr/{station?}")]
    public IActionResult OpenByQr([FromRoute] string? station)
    {
        if (string.IsNullOrWhiteSpace(station))
        {
            return Redirect("/webapp/index.html");
        }

        var encoded = Uri.EscapeDataString(station.Trim());
        return Redirect($"/webapp/index.html?station={encoded}");
    }

    [HttpGet("webapp")]
    public IActionResult OpenWebApp()
    {
        return Redirect("/webapp/index.html");
    }
}
