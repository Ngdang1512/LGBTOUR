using System.Collections.ObjectModel;
using System.Net.Http;
using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;
using SaigonAudioTour.Mobile.Services.Geofencing;
using SaigonAudioTour.Mobile.Services.Realtime;

namespace SaigonAudioTour.Mobile;

public partial class MainPage : ContentPage
{
    private const string NarratingPlaceKey = "NarratingPlaceId";

    private static readonly HttpClient _imageWarmupClient = new()
    {
        Timeout = TimeSpan.FromSeconds(6)
    };

    private List<Place> _allPlaces = new();
    public ObservableCollection<Place> DisplayPlaces { get; set; } = new();

    private PoiApiService? _apiService;
    private GeofencingService? _geofencingService;
    private NarrationEngine? _narrationEngine;
    private UserLogService? _userLogService;
    private ActivityReporterService? _activityReporterService;
    private GeofenceSessionState? _geofenceSessionState;
    private Place? _activePoi;
    private CancellationTokenSource? _geofenceBannerCts;
    private bool _isLoading;
    private bool _isLoadingPlaces;
    private bool _isNavigatingToDetail;
    private bool _geofenceEventsHooked;
    private bool _isGeofenceBannerVisible;
    private string _geofenceBannerBackground = "#EEF2FF";
    private string _geofenceBannerStroke = "#6366F1";
    private string _geofenceBannerTitleColor = "#312E81";
    private string _welcomeText = "Chào mừng bạn 👋";
    private string _routeTitleText = "Tuyến khám phá Sài Gòn";
    private string _routeSectionTitleText = "🚌 Tuyến tham quan nổi bật";

    public string WelcomeText
    {
        get => _welcomeText;
        private set
        {
            if (_welcomeText == value) return;
            _welcomeText = value;
            OnPropertyChanged();
        }
    }

    public string RouteTitleText
    {
        get => _routeTitleText;
        private set
        {
            if (_routeTitleText == value) return;
            _routeTitleText = value;
            OnPropertyChanged();
        }
    }

    public string RouteSectionTitleText
    {
        get => _routeSectionTitleText;
        private set
        {
            if (_routeSectionTitleText == value) return;
            _routeSectionTitleText = value;
            OnPropertyChanged();
        }
    }

    public bool IsGeofenceBannerVisible
    {
        get => _isGeofenceBannerVisible;
        set
        {
            if (_isGeofenceBannerVisible == value) return;
            _isGeofenceBannerVisible = value;
            OnPropertyChanged();
        }
    }

    public string ActivePoiTitle => _activePoi?.Name ?? string.Empty;
    public string ActivePoiSubtitle => _activePoi?.Location ?? string.Empty;
    public string ActivePoiDistanceText { get; private set; } = string.Empty;
    public string ActivePoiBannerTitle => _activePoi == null
        ? string.Empty
        : $"Đang phát thuyết minh tại {_activePoi.Name}";

    public string GeofenceBannerBackground
    {
        get => _geofenceBannerBackground;
        private set
        {
            if (_geofenceBannerBackground == value) return;
            _geofenceBannerBackground = value;
            OnPropertyChanged();
        }
    }

    public string GeofenceBannerStroke
    {
        get => _geofenceBannerStroke;
        private set
        {
            if (_geofenceBannerStroke == value) return;
            _geofenceBannerStroke = value;
            OnPropertyChanged();
        }
    }

    public string GeofenceBannerTitleColor
    {
        get => _geofenceBannerTitleColor;
        private set
        {
            if (_geofenceBannerTitleColor == value) return;
            _geofenceBannerTitleColor = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingPlaces
    {
        get => _isLoadingPlaces;
        set
        {
            if (_isLoadingPlaces == value) return;
            _isLoadingPlaces = value;
            OnPropertyChanged();
        }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        UpdateHeaderTexts();

        if (_isLoading) return;

        _apiService ??= IPlatformApplication.Current?.Services.GetService<PoiApiService>();
        _geofencingService ??= IPlatformApplication.Current?.Services.GetService<GeofencingService>();
        _narrationEngine ??= IPlatformApplication.Current?.Services.GetService<NarrationEngine>();
        _userLogService ??= IPlatformApplication.Current?.Services.GetService<UserLogService>();
        _activityReporterService ??= IPlatformApplication.Current?.Services.GetService<ActivityReporterService>();
        _geofenceSessionState ??= IPlatformApplication.Current?.Services.GetService<GeofenceSessionState>();

        await LoadDataAsync();

        if (_geofencingService != null && !_geofencingService.IsMonitoring)
        {
            if (!_geofenceEventsHooked)
            {
                _geofencingService.OnNearbyPoiDetected += OnNearbyPoiDetected;
                if (_narrationEngine != null)
                {
                    _narrationEngine.OnPlaybackCompleted += OnPlaybackCompleted;
                }

                _geofenceEventsHooked = true;
            }

            await _geofencingService.StartMonitoringAsync();
        }

        if (_activityReporterService != null)
        {
            var userId = Preferences.Get(StorageKeys.UserId, string.Empty);
            await _activityReporterService.StartAsync(userId);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _geofencingService?.StopMonitoring();
        HideGeofenceBanner();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        IsLoadingPlaces = true;
        try
        {
            _apiService ??= IPlatformApplication.Current?.Services.GetService<PoiApiService>();

            if (_apiService != null)
            {
                var places = await _apiService.GetPlacesAsync();

                _allPlaces = (places ?? new List<Place>())
                    .Where(p => p != null)
                    .OrderByDescending(p => p.Priority)
                    .ToList();

                SyncNarratingState();

                DisplayPlaces = new ObservableCollection<Place>(_allPlaces);
                OnPropertyChanged(nameof(DisplayPlaces));

                UpdateHeaderTexts();

                _ = Task.Run(() => WarmupTopImagesAsync(_allPlaces));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", "Không thể tải dữ liệu: " + ex.Message, "OK");
        }
        finally
        {
            IsLoadingPlaces = false;
            _isLoading = false;
        }
    }

    private void FilterPlaces()
    {
        SyncNarratingState();
        DisplayPlaces = new ObservableCollection<Place>(_allPlaces);
        OnPropertyChanged(nameof(DisplayPlaces));
    }

    private void SyncNarratingState()
    {
        var narratingId = Preferences.Get(NarratingPlaceKey, -1);
        foreach (var place in _allPlaces)
        {
            place.IsNarrating = place.Id == narratingId;
        }
    }

    private async void OnPlaceSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Place selectedPlace)
        {
            await Navigation.PushAsync(new DetailPage(selectedPlace));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (_isNavigatingToDetail) return;
        if (sender is not Border card) return;
        if (card.BindingContext is not Place selectedPlace) return;

        _isNavigatingToDetail = true;
        try
        {
            await card.ScaleToAsync(0.975, 70, Easing.CubicOut);
            await card.ScaleToAsync(1, 90, Easing.CubicIn);
            await Navigation.PushAsync(new DetailPage(selectedPlace));
        }
        finally
        {
            _isNavigatingToDetail = false;
        }
    }

    private async void OnNearbyPoiDetected(object? sender, NearbyPoiEventArgs e)
    {
        if (_narrationEngine == null || e.Poi == null)
        {
            return;
        }

        _geofenceBannerCts?.Cancel();
        _geofenceBannerCts?.Dispose();
        _geofenceBannerCts = new CancellationTokenSource();
        var bannerToken = _geofenceBannerCts.Token;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _activePoi = e.Poi;
            ActivePoiDistanceText = e.DistanceMeters < 1000
                ? $"Cách {Math.Round(e.DistanceMeters)} m"
                : $"Cách {e.DistanceMeters / 1000:0.0} km";
            UpdateGeofenceBannerStyle(e.DistanceMeters, e.Poi.TriggerRadius);
            OnPropertyChanged(nameof(ActivePoiTitle));
            OnPropertyChanged(nameof(ActivePoiSubtitle));
            OnPropertyChanged(nameof(ActivePoiDistanceText));
            IsGeofenceBannerVisible = true;
        });

        await _narrationEngine.PlayNarrationAsync(e.Poi);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), bannerToken);
                if (bannerToken.IsCancellationRequested || _activePoi == null)
                {
                    return;
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (_activePoi != null && IsGeofenceBannerVisible)
                    {
                        await Navigation.PushAsync(new DetailPage(_activePoi));
                        HideGeofenceBanner();
                    }
                });
            }
            catch (TaskCanceledException)
            {
                // Banner was dismissed or replaced.
            }
        });
    }

    private async void OnOpenActivePoiDetailClicked(object sender, EventArgs e)
    {
        if (_activePoi == null)
        {
            return;
        }

        await Navigation.PushAsync(new DetailPage(_activePoi));
    }

    private void OnDismissGeofenceBannerClicked(object sender, EventArgs e)
    {
        HideGeofenceBanner();
    }

    private void HideGeofenceBanner()
    {
        _geofenceBannerCts?.Cancel();
        _activePoi = null;
        ActivePoiDistanceText = string.Empty;
        UpdateGeofenceBannerStyle(double.MaxValue, 0);
        IsGeofenceBannerVisible = false;
        _geofenceSessionState?.ClearActivePoi();
        OnPropertyChanged(nameof(ActivePoiTitle));
        OnPropertyChanged(nameof(ActivePoiSubtitle));
        OnPropertyChanged(nameof(ActivePoiDistanceText));
    }

    private async void OnPlaybackCompleted(object? sender, NarrationPlaybackEventArgs e)
    {
        if (_userLogService == null || e == null)
        {
            return;
        }

        var userId = Preferences.Get(StorageKeys.UserId, string.Empty);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var location = await Geolocation.GetLocationAsync();
        if (location == null)
        {
            return;
        }

        await _userLogService.LogNarrationPlaybackAsync(
            int.TryParse(userId, out var parsedUserId) ? parsedUserId : -1,
            e.PoiId,
            location.Latitude,
            location.Longitude,
            e.DurationSeconds);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (_activePoi != null && _activePoi.Id == e.PoiId)
            {
                HideGeofenceBanner();
            }
            else
            {
                _geofenceSessionState?.ClearActivePoi();
            }
        });
    }

    private static async Task WarmupTopImagesAsync(IEnumerable<Place> places)
    {
        var urls = places
            .Select(p => p.ImageUrl)
            .Where(u => !string.IsNullOrWhiteSpace(u) && Uri.TryCreate(u, UriKind.Absolute, out _))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList();

        foreach (var url in urls)                                                                                                                                                                                                       
        {
            try
            {
                using var response = await _imageWarmupClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode) continue;

                await using var stream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[2048];
                _ = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
            }
            catch
            {
                // Bỏ qua lỗi warmup để không ảnh hưởng UI
            }
        }
    }

    private void UpdateGeofenceBannerStyle(double distanceMeters, int triggerRadius)
    {
        if (_activePoi == null)
        {
            GeofenceBannerBackground = "#EEF2FF";
            GeofenceBannerStroke = "#6366F1";
            GeofenceBannerTitleColor = "#312E81";
            return;
        }

        if (distanceMeters <= triggerRadius)
        {
            GeofenceBannerBackground = "#FEE2E2";
            GeofenceBannerStroke = "#DC2626";
            GeofenceBannerTitleColor = "#991B1B";
            return;
        }

        if (distanceMeters <= triggerRadius + 50)
        {
            GeofenceBannerBackground = "#FEF3C7";
            GeofenceBannerStroke = "#F59E0B";
            GeofenceBannerTitleColor = "#92400E";
            return;
        }

        GeofenceBannerBackground = "#EEF2FF";
        GeofenceBannerStroke = "#6366F1";
        GeofenceBannerTitleColor = "#312E81";
    }

    private void UpdateHeaderTexts()
    {
        var languageCode = AppLanguageService.GetAppLanguage();
        var fullName = Preferences.Get(StorageKeys.UserFullName, string.Empty)?.Trim();
        var firstName = string.IsNullOrWhiteSpace(fullName)
            ? string.Empty
            : fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;

        var placeCount = _allPlaces.Count;

        switch (languageCode)
        {
            case "en":
                WelcomeText = string.IsNullOrWhiteSpace(firstName)
                    ? "Welcome back 👋"
                    : $"Welcome back, {firstName} 👋";
                RouteTitleText = "District 1 Discovery Route";
                RouteSectionTitleText = placeCount > 0
                    ? $"🚌 Featured stops ({placeCount})"
                    : "🚌 Featured stops";
                break;

            default:
                WelcomeText = string.IsNullOrWhiteSpace(firstName)
                    ? "Chào bạn trở lại 👋"
                    : $"Chào {firstName}, chúc bạn tour vui vẻ 👋";
                RouteTitleText = "Tuyến khám phá Quận 1";
                RouteSectionTitleText = placeCount > 0
                    ? $"🚌 Tuyến tham quan nổi bật ({placeCount} điểm)"
                    : "🚌 Tuyến tham quan nổi bật";
                break;
        }
    }
}