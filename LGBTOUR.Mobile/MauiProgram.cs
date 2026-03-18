using Microsoft.Extensions.Logging;
using LGBTOUR.Mobile.Services; // Đảm bảo import đúng thư mục chứa TourApiService

namespace LGBTOUR.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Đăng ký Service gọi API (Tồn tại duy nhất trong suốt vòng đời app)
        builder.Services.AddSingleton<TourApiService>();

        // 2. Đăng ký các trang để .NET tự động bơm Service vào
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}