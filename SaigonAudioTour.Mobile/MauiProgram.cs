using Microsoft.Extensions.Logging;
using SaigonAudioTour.Mobile.Services.Geofencing;
using SaigonAudioTour.Mobile.Services;
using SaigonAudioTour.Mobile.Services.Realtime;
using Microsoft.Maui.Devices;
#if ANDROID
using Android.OS;
#endif

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

        // 1. Đăng ký HttpClient gọi API
        builder.Services.AddSingleton(_ =>
        {
            var baseAddress = "http://localhost:5117/";

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Emulator truy cập host qua 10.0.2.2
                // Máy thật truy cập qua adb reverse => localhost:5117
                baseAddress = IsAndroidEmulator()
                    ? "http://10.0.2.2:5117/"
                    : "http://127.0.0.1:5117/";
            }

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
        builder.Services.AddSingleton<ActivityReporterService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static bool IsAndroidEmulator()
    {
#if ANDROID
        var fingerprint = Build.Fingerprint?.ToLowerInvariant() ?? string.Empty;
        var model = Build.Model?.ToLowerInvariant() ?? string.Empty;
        var product = Build.Product?.ToLowerInvariant() ?? string.Empty;
        var manufacturer = Build.Manufacturer?.ToLowerInvariant() ?? string.Empty;

        return fingerprint.Contains("generic")
               || fingerprint.Contains("emulator")
               || model.Contains("emulator")
               || model.Contains("sdk_gphone")
               || product.Contains("sdk")
               || manufacturer.Contains("genymotion");
#else
        return false;
#endif
    }
}