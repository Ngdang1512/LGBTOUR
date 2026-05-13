using SaigonAudioTour.Api.DTOs.Auth;
using SaigonAudioTour.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("login")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var auth = await _authService.LoginAsync(dto);
            if (auth == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

            return Ok(auth);
        }

        [HttpPost("admin-login")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> AdminLogin([FromBody] LoginDto dto)
        {
            var auth = await _authService.AdminLoginAsync(dto);
            if (auth == null)
            {
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu quản trị." });
            }

            return Ok(auth);
        }

        [HttpPost("admin-refresh")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> AdminRefresh()
        {
            var username = User?.Identity?.Name ?? string.Empty;
            var refreshed = await _authService.RefreshAdminTokenAsync(username);

            if (refreshed == null)
            {
                return Unauthorized(new { message = "Không thể refresh token quản trị." });
            }

            return Ok(refreshed);
        }

        [HttpPost("register")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var created = await _authService.RegisterAsync(dto);
            if (created == null)
            {
                return BadRequest(new { message = "Email đã tồn tại hoặc dữ liệu không hợp lệ." });
            }

            return Ok(created);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            // Người dùng chỉ được xem profile của chính mình, trừ Admin
            var callerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                           ?? User.FindFirst("email")?.Value
                           ?? User.Identity?.Name;

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !string.Equals(callerEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var profile = await _authService.GetProfileByEmailAsync(email);
            if (profile == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            return Ok(new
            {
                fullName = profile.FullName,
                email = profile.Email,
                avatarUrl = string.Empty,
                userId = profile.UserId,
                subscriptionStatus = profile.SubscriptionStatus
            });
        }
    }
}