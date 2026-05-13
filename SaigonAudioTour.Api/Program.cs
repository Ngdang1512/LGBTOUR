using SaigonAudioTour.Api.Data;
using SaigonAudioTour.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using SaigonAudioTour.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES
builder.Services.AddScoped<IPoiService, PoiService>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<INarrationService, NarrationService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var ttsProvider = builder.Configuration["Tts:Provider"]?.Trim().ToLowerInvariant();
var useMockTone = builder.Configuration.GetValue<bool>("Tts:UseMockTone");
if (ttsProvider == "azure")
{
    builder.Services.AddHttpClient<ITtsService, AzureSpeechRestTtsService>();
}
else if (builder.Environment.IsDevelopment() && useMockTone)
{
    builder.Services.AddScoped<ITtsService, MockTtsService>();
}
else
{
    builder.Services.AddScoped<ITtsService, NoopTtsService>();
}
builder.Services.AddSingleton<SubscriptionStore>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<ActivityPresenceCleanupService>();

// Payment Gateway Services
builder.Services.AddScoped<IPaymentGatewayService, VNPayAdapter>();
builder.Services.AddScoped<IPaymentGatewayOrchestrator, PaymentGatewayOrchestrator>();
builder.Services.AddHttpClient<VNPayAdapter>();

// Security & RBAC Services
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();

// 2. DATABASE
var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
var useSqlServer = DatabaseHelper.CanConnectSqlServer(sqlConnection);
var sqlitePathConfig = builder.Configuration.GetValue<string>("DatabaseSettings:SqlitePath");
var sqlitePath = DatabaseHelper.ResolveSqlitePath(builder.Environment.ContentRootPath, sqlitePathConfig);
Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(sqlConnection);
    }
    else
    {
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

var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddSignalR().AddStackExchangeRedis(redisConnection);
    builder.Services.AddSingleton<IUserActivityStore>(sp =>
        new RedisUserActivityStore(
            redisConnection,
            sp.GetRequiredService<ILogger<RedisUserActivityStore>>()));
}
else
{
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<IUserActivityStore, InMemoryUserActivityStore>();
}

// 5. RATE LIMITING — chống brute-force login và spam API
builder.Services.AddRateLimiter(options =>
{
    // Giới hạn chặt cho login/register: 10 lần / phút / IP
    options.AddFixedWindowLimiter("auth", policy =>
    {
        policy.Window        = TimeSpan.FromMinutes(1);
        policy.PermitLimit   = 10;
        policy.QueueLimit    = 0;
        policy.AutoReplenishment = true;
    });

    // Giới hạn vừa cho các endpoint công khai ghi dữ liệu: 60 lần / phút / IP
    options.AddFixedWindowLimiter("write_public", policy =>
    {
        policy.Window        = TimeSpan.FromMinutes(1);
        policy.PermitLimit   = 60;
        policy.QueueLimit    = 0;
        policy.AutoReplenishment = true;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, _) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            "{\"message\":\"Quá nhiều yêu cầu. Vui lòng thử lại sau.\"}");
    };
});

// 6. CORS — AllowCredentials bắt buộc cho SignalR WebSocket
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5200", "http://localhost:5244" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.SetIsOriginAllowed(_ => true);
        else
            policy.WithOrigins(allowedOrigins);

        policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

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
        try
        {
            context.Database.Migrate();
        }
        catch
        {
            context.Database.EnsureCreated();
        }
    }

    DataSeeder.SeedAll(context, useSqlServer);
}

// 8. MIDDLEWARE PIPELINE (THỨ TỰ BẮT BUỘC)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SaigonAudioTour API v1"));
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ActivityHub>("/hubs/activity");
app.MapHub<TelemetryHub>("/hubs/telemetry");
app.Run();
