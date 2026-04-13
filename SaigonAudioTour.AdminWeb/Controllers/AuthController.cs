using SaigonAudioTour.AdminWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace SaigonAudioTour.AdminWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã có Cookie (đã đăng nhập) thì cho vô thẳng Dashboard
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");

            // 1. Gửi request Login tới API
            var response = await client.PostAsJsonAsync("api/Auth/admin-login", new
            {
                username = model.Username,
                password = model.Password
            });

            if (response.IsSuccessStatusCode)
            {
                // 2. Đọc cục JSON API trả về (chứa Token)
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                string token = result.GetProperty("token").GetString() ?? "";

                // 3. Cất Token vào Cookie của trình duyệt Web Admin
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim("JWToken", token) // Mấu chốt là cái này!
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // 4. Thành công thì phi thẳng vào màn hình chính
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}