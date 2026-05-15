using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SaigonAudioTour.AdminWeb.Middleware;

public sealed class AdminJwtRefreshMiddleware
{
    private static readonly TimeSpan RefreshBeforeExpiry = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan MinRefreshInterval = TimeSpan.FromMinutes(2);

    private readonly RequestDelegate _next;

    public AdminJwtRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var token = context.User.FindFirst("JWToken")?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            await _next(context);
            return;
        }

        if (!ShouldRefreshToken(context.User, token))
        {
            await _next(context);
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("api/auth/admin-refresh", null);
            if (!response.IsSuccessStatusCode)
            {
                await _next(context);
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            if (!doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                await _next(context);
                return;
            }

            var newToken = tokenElement.GetString();
            if (string.IsNullOrWhiteSpace(newToken))
            {
                await _next(context);
                return;
            }

            var claims = context.User.Claims
                .Where(c => c.Type != "JWToken" && c.Type != "JWTokenRefreshedAt")
                .ToList();

            claims.Add(new Claim("JWToken", newToken));
            claims.Add(new Claim("JWTokenRefreshedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            context.User = principal;
        }
        catch
        {
            // Ignore refresh failures and continue request with current token.
        }

        await _next(context);
    }

    private static bool ShouldRefreshToken(ClaimsPrincipal user, string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return false;
        }

        var jwt = handler.ReadJwtToken(token);
        var expiresUtc = jwt.ValidTo;
        if (expiresUtc <= DateTime.UtcNow)
        {
            return true;
        }

        var remaining = expiresUtc - DateTime.UtcNow;
        if (remaining > RefreshBeforeExpiry)
        {
            return false;
        }

        var refreshedAtClaim = user.FindFirst("JWTokenRefreshedAt")?.Value;
        if (!string.IsNullOrWhiteSpace(refreshedAtClaim)
            && long.TryParse(refreshedAtClaim, out var refreshedAtUnix))
        {
            var refreshedAt = DateTimeOffset.FromUnixTimeSeconds(refreshedAtUnix);
            if (DateTimeOffset.UtcNow - refreshedAt < MinRefreshInterval)
            {
                return false;
            }
        }

        return true;
    }
}
