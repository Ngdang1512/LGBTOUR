namespace SaigonAudioTour.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Services.AppLanguageService.LanguageChanged += OnLanguageChanged;
        RefreshLocalizedTitles();
        
        Routing.RegisterRoute("DetailPage", typeof(DetailPage));
        Routing.RegisterRoute(nameof(UpgradePage), typeof(UpgradePage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Services.AppLanguageService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, string languageCode)
    {
        MainThread.BeginInvokeOnMainThread(RefreshLocalizedTitles);
    }

    private void RefreshLocalizedTitles()
    {
        var isVi = Services.AppLanguageService.GetAppLanguage() == "vi";

        MainShellContent.Title = isVi ? "Khám phá" : "Explore";
        MapShellContent.Title = isVi ? "Bản đồ" : "Map";
        SettingsShellContent.Title = isVi ? "Cài đặt" : "Settings";
    }
}