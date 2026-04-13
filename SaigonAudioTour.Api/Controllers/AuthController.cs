using SaigonAudioTour.Api.DTOs.Auth;
using SaigonAudioTour.Api.Services;
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
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var auth = await _authService.LoginAsync(dto);
            if (auth == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

            return Ok(auth);
        }

        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin([FromBody] LoginDto dto)
        {
            var auth = await _authService.AdminLoginAsync(dto);
            if (auth == null)
            {
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu quản trị." });
            }

            return Ok(auth);
        }

        [HttpPost("register")]
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
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
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
                userId = profile.UserId
            });
        }
    }
}