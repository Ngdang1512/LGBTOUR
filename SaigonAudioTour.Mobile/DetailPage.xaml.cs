using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;
using SaigonAudioTour.Mobile.Services.Geofencing;
using Microsoft.Maui.Media;

namespace SaigonAudioTour.Mobile;

public partial class DetailPage : ContentPage
{
    private const string NarratingPlaceKey = "NarratingPlaceId";
    private readonly SubscriptionApiService _subscriptionApiService;
    private readonly GeofenceSessionState? _geofenceSessionState;

    public Place? SelectedPlace { get; set; }
    
    // Biến dùng để dừng âm thanh ngay lập tức
    private CancellationTokenSource _cts = new();
    private bool _hasAutoPlayed;

    public DetailPage(Place? place = null)
    {
        InitializeComponent();
        SelectedPlace = place;
        BindingContext = this; 

        _subscriptionApiService = IPlatformApplication.Current?.Services.GetService<SubscriptionApiService>()
            ?? throw new InvalidOperationException("SubscriptionApiService chưa được đăng ký DI.");
        _geofenceSessionState = IPlatformApplication.Current?.Services.GetService<GeofenceSessionState>();

        // Khởi tạo danh sách ngôn ngữ cho Picker
        LanguagePicker.ItemsSource = new List<string> { 
            "🇻🇳 Tiếng Việt", 
            "🇬🇧 English",
            "🇨🇳 中文",
            "🇯🇵 日本語",
            "🇰🇷 한국어",
            "🇫🇷 Français"
        };

        var savedLanguage = Preferences.Get(StorageKeys.NarrationLanguage, AppLanguageService.GetAppLanguage());
        LanguagePicker.SelectedIndex = savedLanguage switch
        {
            "en" => 1,
            "zh" => 2,
            "ja" => 3,
            "ko" => 4,
            "fr" => 5,
            _ => 0
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (SelectedPlace != null)
        {
            _geofenceSessionState?.SetActivePoi(
                SelectedPlace,
                0,
                _geofenceSessionState?.CurrentLocation);
            _geofenceSessionState?.SetActivityStatus("viewing_detail");
        }

        if (_hasAutoPlayed || SelectedPlace == null)
        {
            return;
        }

        var userId = Preferences.Get(StorageKeys.UserId, string.Empty);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var premiumStatus = await _subscriptionApiService.GetPremiumStatusAsync(userId);
        if (premiumStatus?.IsPremium != true)
        {
            return;
        }

        _hasAutoPlayed = true;
        await Task.Delay(250);
        OnPlayClicked(this, EventArgs.Empty);
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        if (SelectedPlace == null) return;

        // Đổi giao diện: Ẩn nút Phát, Hiện nút Dừng
        PlayButton.IsVisible = false;
        StopButton.IsVisible = true;

        _cts = new CancellationTokenSource();
        string textToRead = SelectedPlace.TtsScript;
        var selectedLang = LanguagePicker.SelectedItem as string;
        var selectedCode = GetLanguageCode(selectedLang);
        AppLanguageService.SetNarrationLanguage(selectedCode);
        Preferences.Set(NarratingPlaceKey, SelectedPlace.Id);
        _geofenceSessionState?.SetActivityStatus("listening");

        // Demo translation theo ngôn ngữ đã chọn
        textToRead = selectedCode switch
        {
            "en" => TranslateToEnglish(),
            "zh" => TranslateToChinese(),
            "ja" => TranslateToJapanese(),
            "ko" => TranslateToKorean(),
            "fr" => TranslateToFrench(),
            _ => SelectedPlace.TtsScript
        };

        try
        {
            var locale = await ResolveLocaleAsync(selectedCode);
            var speechOptions = locale != null ? new SpeechOptions { Locale = locale } : null;

            // Ra lệnh cho thiết bị phát âm thanh
            await TextToSpeech.Default.SpeakAsync(textToRead, speechOptions, _cts.Token);
            
            // Cập nhật progress bar
            await AnimateProgressBar();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", $"Không thể phát âm thanh: {ex.Message}", "OK");
        }
        finally
        {
            // Khi đọc xong, trả giao diện về như cũ
            PlayButton.IsVisible = true;
            StopButton.IsVisible = false;
            AudioProgressBar.Progress = 0;
            ClearNarratingFlag();
        }
    }

    private async Task AnimateProgressBar()
    {
        // Mô phỏng progress bar animation nhẹ hơn cho UI thread
        for (int i = 0; i <= 40; i++)
        {
            if (_cts?.IsCancellationRequested == true) break;
            await AudioProgressBar.ProgressTo(i / 40.0, 120, Easing.Linear);
        }
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        // Hủy lệnh đọc ngay lập tức
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        ClearNarratingFlag();
    }

    private async void OnCallClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Gọi Quán", "Chức năng sẽ được cập nhật", "OK");
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (SelectedPlace == null) return;

        if (SelectedPlace.Latitude > 0 && SelectedPlace.Longitude > 0)
        {
            var location = new Location(SelectedPlace.Latitude, SelectedPlace.Longitude);
            await Map.Default.OpenAsync(location, new MapLaunchOptions { Name = SelectedPlace.Name });
        }
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        if (SelectedPlace == null) return;

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = SelectedPlace.Name,
            Text = $"Khám phá {SelectedPlace.Name} cùng SaigonAudioTour!\n{SelectedPlace.TtsScript}",
            Uri = "https://lgbtour.app"
        });
    }

    private static string GetLanguageCode(string? selectedLang)
    {
        if (string.IsNullOrWhiteSpace(selectedLang)) return "vi";
        if (selectedLang.Contains("English")) return "en";
        if (selectedLang.Contains("中文")) return "zh";
        if (selectedLang.Contains("日本")) return "ja";
        if (selectedLang.Contains("한국어")) return "ko";
        if (selectedLang.Contains("Français")) return "fr";
        return "vi";
    }

    private static async Task<Locale?> ResolveLocaleAsync(string languageCode)
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        return locales.FirstOrDefault(l => l.Language.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase));
    }

    // Hàm dịch demo theo ngôn ngữ
    private string TranslateToEnglish()
    {
        if (SelectedPlace == null) return string.Empty;
        return $"Welcome to {SelectedPlace.Name}, a famous destination in Ho Chi Minh City. Address: {SelectedPlace.Location}.";
    }

    private string TranslateToChinese()
    {
        if (SelectedPlace == null) return string.Empty;
        return $"欢迎来到{SelectedPlace.Name}，这是胡志明市著名景点。地址：{SelectedPlace.Location}。";
    }

    private string TranslateToJapanese()
    {
        if (SelectedPlace == null) return string.Empty;
        return $"{SelectedPlace.Name}へようこそ。ホーチミン市の有名な観光地です。住所は{SelectedPlace.Location}です。";
    }

    private string TranslateToKorean()
    {
        if (SelectedPlace == null) return string.Empty;
        return $"{SelectedPlace.Name}에 오신 것을 환영합니다. 호치민시의 유명한 관광지이며 주소는 {SelectedPlace.Location}입니다.";
    }

    private string TranslateToFrench()
    {
        if (SelectedPlace == null) return string.Empty;
        return $"Bienvenue à {SelectedPlace.Name}, un lieu célèbre de Hô Chi Minh-Ville. Adresse : {SelectedPlace.Location}.";
    }

    private void ClearNarratingFlag()
    {
        if (SelectedPlace == null) return;

        var currentId = Preferences.Get(NarratingPlaceKey, -1);
        if (currentId == SelectedPlace.Id)
        {
            Preferences.Set(NarratingPlaceKey, -1);
        }
    }

    // Nếu người dùng đang nghe mà bấm nút Quay lại (Back), tự động tắt tiếng
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        ClearNarratingFlag();

        if (SelectedPlace != null)
        {
            _geofenceSessionState?.SetActivityStatus("viewing_detail");
        }

        if (SelectedPlace != null && _geofenceSessionState?.ActivePoi?.Id == SelectedPlace.Id)
        {
            _geofenceSessionState.ClearActivePoi();
        }
    }
}