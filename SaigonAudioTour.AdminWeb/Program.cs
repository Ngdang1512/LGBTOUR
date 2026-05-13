using Microsoft.AspNetCore.Authentication.Cookies;
using SaigonAudioTour.AdminWeb.Handlers;
using SaigonAudioTour.AdminWeb.Middleware;
using SaigonAudioTour.AdminWeb.Options;

namespace SaigonAudioTour.AdminWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<RealtimeOptions>(builder.Configuration.GetSection("Realtime"));
            var apiBaseUrl = builder.Configuration["Realtime:ApiBaseUrl"] ?? "http://localhost:5117";

            // 1. CẤU HÌNH API — BearerTokenHandler tự động gắn JWT của admin vào mọi request
            builder.Services.AddTransient<BearerTokenHandler>();
            builder.Services.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri($"{apiBaseUrl.TrimEnd('/')}/");
            })
            .AddHttpMessageHandler<BearerTokenHandler>();

            // 2. CẤU HÌNH COOKIE
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/Auth/Login";
                options.Cookie.Name = "SaigonTourAdminCookie";
                options.ExpireTimeSpan = TimeSpan.FromHours(4);
            });

            // DÒNG NÀY LÚC NÃY ÔNG LỠ XÓA NÈ:
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // 3. KÍCH HOẠT BẢO MẬT
            app.UseAuthentication();
            app.UseMiddleware<AdminJwtRefreshMiddleware>();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}