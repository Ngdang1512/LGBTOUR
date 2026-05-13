namespace SaigonAudioTour.Mobile;

public partial class AppShell : Shell
{
    private readonly Services.UserLogService? _userLogService;
    private readonly Services.Realtime.ActivityReporterService? _activityReporterService;
    private IDispatcherTimer? _heartbeatTimer;

    public AppShell()
    {
        InitializeComponent();
        _userLogService = IPlatformApplication.Current?.Services.GetService<Services.UserLogService>();
        _activityReporterService = IPlatformApplication.Current?.Services.GetService<Services.Realtime.ActivityReporterService>();
        Services.AppLanguageService.LanguageChanged += OnLanguageChanged;
        RefreshLocalizedTitles();
        
        Routing.RegisterRoute("DetailPage", typeof(DetailPage));
        Routing.RegisterRoute(nameof(UpgradePage), typeof(UpgradePage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartHeartbeatTimer();
        _ = StartRealtimeReporterAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Services.AppLanguageService.LanguageChanged -= OnLanguageChanged;
        StopHeartbeatTimer();
    }

    private async Task StartRealtimeReporterAsync()
    {
        if (_activityReporterService == null)
        {
            return;
        }

        var userId = Preferences.Get(Services.StorageKeys.UserId, string.Empty);
        await _activityReporterService.StartAsync(userId);
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

    private void StartHeartbeatTimer()
    {
        if (_userLogService == null || Dispatcher == null)
        {
            return;
        }

        _heartbeatTimer ??= Dispatcher.CreateTimer();
        _heartbeatTimer.Interval = TimeSpan.FromSeconds(30);
        _heartbeatTimer.IsRepeating = true;
        _heartbeatTimer.Tick -= OnHeartbeatTick;
        _heartbeatTimer.Tick += OnHeartbeatTick;

        if (!_heartbeatTimer.IsRunning)
        {
            _heartbeatTimer.Start();
        }

        _ = SendHeartbeatAsync();
    }

    private void StopHeartbeatTimer()
    {
        if (_heartbeatTimer == null)
        {
            return;
        }

        _heartbeatTimer.Tick -= OnHeartbeatTick;
        _heartbeatTimer.Stop();
    }

    private async void OnHeartbeatTick(object? sender, EventArgs e)
    {
        await SendHeartbeatAsync();
    }

    private async Task SendHeartbeatAsync()
    {
        if (_userLogService == null)
        {
            return;
        }

        var userId = Preferences.Get(Services.StorageKeys.UserId, string.Empty);
        await _userLogService.SendHeartbeatAsync(userId);
    }
}