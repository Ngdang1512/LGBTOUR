using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AuthController : Controller
{
    private readonly HttpClient _httpClient;

    public AuthController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7xxx/"); // Thay bằng cổng API của bạn
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(); // Trả về giao diện Form Login
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Gọi lên API của bạn
        var response = await _httpClient.PostAsync($"/api/auth/login?username={username}&password={password}", null);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            var token = result.token;

            // Tạo Cookie lưu Token lại trên trình duyệt Web
            var claims = new List<Claim> { new Claim("jwt", token) };
            var identity = new ClaimsIdentity(claims, "AdminCookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("AdminCookies", principal);

            return RedirectToAction("Index", "Home"); // Đăng nhập thành công, vào Dashboard
        }

        ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
        return View();
    }
}

// Class phụ để hứng dữ liệu JSON từ API trả về
public class LoginResponse { public string token { get; set; } }