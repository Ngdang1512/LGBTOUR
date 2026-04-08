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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);

            if (token == null)
                return Unauthorized("Tài khoản hoặc mật khẩu không chính xác.");

            return Ok(new { Token = token, Message = "Đăng nhập thành công!" });
        }
    }
}