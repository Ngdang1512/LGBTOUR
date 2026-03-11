using LGBTOUR.Api.Interfaces;
using LGBTOUR.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using LGBTOUR.Api.Data; // Import namespace của thư mục Data vừa tạo

var builder = WebApplication.CreateBuilder(args);

// --- 1. Cấu hình Kết nối Database ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllers(); // Thêm cái này để hỗ trợ viết API bằng Controller sau này
builder.Services.AddOpenApi();
// Đăng ký Dependency Injection
builder.Services.AddScoped<IPOIRepository, POIRepository>();
builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --- 2. Map các Controller (Quan trọng cho AdminApp gọi API) ---
// Giữ lại phần WeatherForecast mặc định để bạn test thử API có chạy không
var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
app.MapGet("/weatherforecast", () => {
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        Random.Shared.Next(-20, 55), summaries[Random.Shared.Next(summaries.Length)]))
        .ToArray();
    return forecast;
}).WithName("GetWeatherForecast");
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}