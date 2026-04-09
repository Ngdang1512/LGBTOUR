using LGBTOUR.Api.DTOs.Auth;
using LGBTOUR.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LGBTOUR.Api.Controllers
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
            var token = await _authService.LoginAsync(dto);
            if (token == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

            return Ok(new { token = token }); // Trả về object chứa token
        }
    }
}