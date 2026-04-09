using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SaigonAudioTour.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            // Tạm thời fix cứng tài khoản Admin để làm Đồ án cho nhanh
            if (username == "admin" && password == "admin123")
            {
                // 1. Ghi thông tin người dùng lên thẻ
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                // 2. Lấy con dấu xác nhận (Phải giống hệt chuỗi bên Program.cs)
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SaigonAudioTour_Super_Secret_Key_For_Admin_Only_12345!"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // 3. In Thẻ từ (Cài đặt hạn sử dụng thẻ là 1 ngày)
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

                // 4. Trả thẻ (chuỗi Token) về cho người dùng
                return Ok(new
                {
                    message = "Đăng nhập thành công!",
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }

            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });
        }
    }
}