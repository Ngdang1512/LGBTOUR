using System.Net.Http.Headers;
using System.Security.Claims;

namespace SaigonAudioTour.AdminWeb.Handlers;

/// <summary>
/// DelegatingHandler tự động gắn JWT của admin đang đăng nhập vào mọi request
/// gửi đến API backend. Token được đọc từ claim "JWToken" trong cookie session.
/// </summary>
public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;

    public BearerTokenHandler(IHttpContextAccessor ctx)
    {
        _ctx = ctx;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _ctx.HttpContext?.User.FindFirst("JWToken")?.Value;
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
