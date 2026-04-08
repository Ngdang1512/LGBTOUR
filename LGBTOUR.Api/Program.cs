using LGBTOUR.Api.Data;
using LGBTOUR.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ SERVICES (DEPENDENCY INJECTION) ---
builder.Services.AddScoped<IPoiService, PoiService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<INarrationService, NarrationService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// --- 2. KẾT NỐI DATABASE (SQL SERVER) ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 3. CẤU HÌNH CONTROLLERS & JSON ---
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Ngăn chặn vòng lặp vô hạn khi trả về dữ liệu có quan hệ n-n
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// --- 4. CẤU HÌNH BẢO MẬT JWT ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"] ?? "LGBTOUR_Super_Secret_Key_For_Admin_Only_12345!";
var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Có thể set thành true nếu cấu hình Issuer trong appsettings
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

// --- 5. CẤU HÌNH CORS (CHO PHÉP FRONTEND GỌI API) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// --- 6. CẤU HÌNH SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LGBTOUR API - Phố Ẩm Thực Quận 4", Version = "v1" });

    // Cấu hình nút "Authorize" để nhập Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Token theo định dạng: **Bearer {your_token}**",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// --- 7. TỰ ĐỘNG MIGRATION & SEED DATA (KHỞI TẠO DATABASE) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Tự động tạo database/bảng nếu chưa có
        context.Database.Migrate();

        // Kiểm tra và tạo tài khoản Admin mặc định
        if (!context.Admins.Any())
        {
            context.Admins.Add(new LGBTOUR.Api.Entities.Admin
            {
                Username = "admin",
                // Mật khẩu mặc định: admin123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "Quản trị viên hệ thống"
            });
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Có lỗi xảy ra khi khởi tạo dữ liệu (Seed Data).");
    }
}

// --- 8. PIPELINE XỬ LÝ REQUEST ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LGBTOUR API v1");
    });
}

// Quan trọng: Thứ tự các Middleware bên dưới phải chính xác
app.UseHttpsRedirection();

// Phục vụ file tĩnh (Ảnh quán ăn, file âm thanh thuyết minh trong wwwroot)
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication(); // Phải nằm trước Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();