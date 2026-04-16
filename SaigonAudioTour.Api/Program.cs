using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Entities;
using SaigonAudioTour.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES
builder.Services.AddScoped<IPoiService, PoiService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<INarrationService, NarrationService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITtsService, MockTtsService>();
builder.Services.AddSingleton<SubscriptionStore>();

// 2. DATABASE
var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
var useSqlServer = CanConnectSqlServer(sqlConnection);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(sqlConnection);
    }
    else
    {
        var sqlitePath = Path.Combine(AppContext.BaseDirectory, "saigonaudiotour.db");
        options.UseSqlite($"Data Source={sqlitePath}");
    }
});

// 3. CONTROLLERS
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 4. JWT & AUTHENTICATION
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero // Sửa lỗi delay token
        };
    });

builder.Services.AddAuthorization();

// 5. CORS
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// 6. SWAGGER (Tự động chèn Bearer)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SaigonAudioTour API Quận 1", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Dán mã Token vào ô bên dưới (Không cần gõ Bearer)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

var app = builder.Build();

// 7. KHỞI TẠO DATABASE & DỮ LIỆU GỐC
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (useSqlServer)
    {
        context.Database.Migrate();
    }
    else
    {
        context.Database.EnsureCreated();
    }

    if (!context.Admins.Any(a => a.Username == "MinhVy"))
    {
        context.Admins.Add(new Admin
        {
            Username = "MinhVy",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            FullName = "Admin Tuyến Xe Buýt"
        });
        context.SaveChanges();
    }

    EnsureUsersTable(context, useSqlServer);
    MigrateLegacyUsers(context);

    SeedDefaultPois(context);
}

// 8. MIDDLEWARE PIPELINE (THỨ TỰ BẮT BUỘC)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SaigonAudioTour API v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Cho phép mở file Audio/Image
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

static bool CanConnectSqlServer(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString)) return false;

    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 2
        };

        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();
        return true;
    }
    catch
    {
        return false;
    }
}

static void SeedDefaultPois(ApplicationDbContext context)
{
    var defaultPois = new[]
    {
        new { Name = "Chợ Bến Thành", Description = "Phường Bến Thành, Quận 1, TP.HCM", Lat = 10.77252, Lng = 106.69805, Radius = 70, Priority = 10, IsStopStation = true },
        new { Name = "Nhà hát Thành phố", Description = "Số 7 Công trường Lam Sơn, Quận 1, TP.HCM", Lat = 10.77662, Lng = 106.70326, Radius = 65, Priority = 9, IsStopStation = true },
        new { Name = "Trụ sở Ủy ban Nhân dân TP.HCM", Description = "Số 86 Lê Thánh Tôn, Quận 1, TP.HCM", Lat = 10.77695, Lng = 106.69949, Radius = 60, Priority = 8, IsStopStation = true },
        new { Name = "Dinh Độc Lập", Description = "135 Nam Kỳ Khởi Nghĩa, Quận 1, TP.HCM", Lat = 10.77713, Lng = 106.69531, Radius = 60, Priority = 7, IsStopStation = true },
        new { Name = "Nhà thờ Đức Bà", Description = "Số 1 Công xã Paris, Quận 1, TP.HCM", Lat = 10.77978, Lng = 106.69902, Radius = 58, Priority = 6, IsStopStation = true },
        new { Name = "Bưu điện Thành phố", Description = "Số 2 Công xã Paris, Quận 1, TP.HCM", Lat = 10.77956, Lng = 106.70049, Radius = 55, Priority = 5, IsStopStation = true },
        new { Name = "Bảo tàng Chứng tích Chiến tranh", Description = "28 Võ Văn Tần, Quận 3, TP.HCM", Lat = 10.77937, Lng = 106.69134, Radius = 55, Priority = 4, IsStopStation = true },
        new { Name = "Bảo tàng Lịch sử", Description = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM", Lat = 10.78693, Lng = 106.70252, Radius = 55, Priority = 3, IsStopStation = true },
        new { Name = "Thảo Cầm Viên Sài Gòn", Description = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM", Lat = 10.78743, Lng = 106.70452, Radius = 60, Priority = 2, IsStopStation = true },
        new { Name = "Saigon Skydeck", Description = "Tầng 49, Bitexco Financial Tower, Quận 1, TP.HCM", Lat = 10.77168, Lng = 106.70411, Radius = 60, Priority = 1, IsStopStation = true }
    };

    foreach (var seedPoi in defaultPois)
    {
        var existingPoi = context.POIs.FirstOrDefault(p => p.Name == seedPoi.Name);
        if (existingPoi == null)
        {
            context.POIs.Add(new POI
            {
                Name = seedPoi.Name,
                Description = seedPoi.Description,
                Lat = seedPoi.Lat,
                Lng = seedPoi.Lng,
                Radius = seedPoi.Radius,
                Priority = seedPoi.Priority,
                IsStopStation = seedPoi.IsStopStation
            });
            continue;
        }

        existingPoi.Description = seedPoi.Description;
        existingPoi.Lat = seedPoi.Lat;
        existingPoi.Lng = seedPoi.Lng;
        existingPoi.Radius = seedPoi.Radius;
        existingPoi.Priority = seedPoi.Priority;
        existingPoi.IsStopStation = seedPoi.IsStopStation;
    }

    context.SaveChanges();
}

static void EnsureUsersTable(ApplicationDbContext context, bool useSqlServer)
{
    if (useSqlServer)
    {
        return;
    }

    context.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    AvatarUrl TEXT NULL
);");

    context.Database.ExecuteSqlRaw(@"
CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);");
}

static void MigrateLegacyUsers(ApplicationDbContext context)
{
    var legacyUsers = context.Admins
        .Where(admin => admin.Username.Contains('@'))
        .Select(admin => new
        {
            Email = admin.Username.Trim().ToLowerInvariant(),
            admin.PasswordHash,
            FullName = string.IsNullOrWhiteSpace(admin.FullName) ? admin.Username : admin.FullName
        })
        .ToList();

    if (legacyUsers.Count == 0)
    {
        return;
    }

    var existingEmails = context.Users
        .AsNoTracking()
        .Select(user => user.Email)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var imported = false;

    foreach (var legacyUser in legacyUsers)
    {
        if (existingEmails.Contains(legacyUser.Email))
        {
            continue;
        }

        context.Users.Add(new AppUser
        {
            Email = legacyUser.Email,
            PasswordHash = legacyUser.PasswordHash,
            FullName = legacyUser.FullName,
            AvatarUrl = null,
            SubscriptionStatus = "free"
        });

        existingEmails.Add(legacyUser.Email);
        imported = true;
    }

    if (imported)
    {
        context.SaveChanges();
    }
}