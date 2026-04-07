using Microsoft.Extensions.Logging;
using LGBTOUR.Mobile.Services;
using Microsoft.Maui.Controls.Hosting; // Đảm bảo import đúng thư mục chứa TourApiService

namespace LGBTOUR.Mobile;

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

        // 1. Đăng ký Service gọi API (Tồn tại duy nhất trong suốt vòng đời app)
        builder.Services.AddSingleton<TourApiService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}