using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LGBTOUR.AdminWeb.Controllers
{
    public class AuthController : Controller
    {
        // Hiện trang Đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì cho vào thẳng Dashboard
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // Xử lý khi bấm nút Đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // TÀI KHOẢN MẶC ĐỊNH (Bạn có thể đổi lại tùy ý)
            if (username == "admin" && password == "123456")
            {
                // Tạo chứng minh thư (Claims) cho Admin
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Quản Trị Viên"),
                    new Claim(ClaimTypes.Role, "Administrator")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Lưu vào Cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home"); // Đăng nhập xong vào trang chủ
            }

            // Nếu sai pass
            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View();
        }

        // Đăng xuất
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}