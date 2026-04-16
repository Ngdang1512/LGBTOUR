using Microsoft.Extensions.Logging;
using SaigonAudioTour.Mobile.Services.Geofencing;
using SaigonAudioTour.Mobile.Services;
using Microsoft.Maui.Devices;

namespace SaigonAudioTour.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Đăng ký HttpClient gọi API (Android emulator dùng 10.0.2.2)
        builder.Services.AddSingleton(_ =>
        {
            var baseAddress = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5117/"
                : "http://localhost:5117/";

            return new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(12)
            };
        });

        // 2. Đăng ký các service gọi API thật theo từng module
        builder.Services.AddSingleton<AuthApiService>();
        builder.Services.AddSingleton<PoiApiService>();
        builder.Services.AddSingleton<SubscriptionApiService>();
        builder.Services.AddSingleton<NarrationApiService>();
        builder.Services.AddSingleton<UserLogService>();

        // 3. Đăng ký Geofencing & Narration Engine
        builder.Services.AddSingleton<GeofenceSessionState>();
        builder.Services.AddSingleton<GeofencingService>();
        builder.Services.AddSingleton<NarrationEngine>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}