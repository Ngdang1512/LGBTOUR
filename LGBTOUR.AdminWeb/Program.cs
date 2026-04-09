using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm dịch vụ MVC (Controllers và Views)
builder.Services.AddControllersWithViews();

// 2. Cấu hình HttpClient để gọi API (Sửa lại port 5117 hoặc 7092 cho đúng với API của ông)
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7092/");
});

// 3. Cấu hình xác thực bằng Cookie (Lưu Token JWT)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "LgbTourAdminSession";
        options.LoginPath = "/Auth/Login"; // Nếu chưa đăng nhập, tự động đá về trang này
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(4); // Sống 4 tiếng
    });

var app = builder.Build();

// 4. Pipeline xử lý request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Cho phép dùng CSS, JS, Images trong wwwroot

app.UseRouting();

// BẮT BUỘC: Authentication phải đứng trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// 5. Cấu hình đường dẫn mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();