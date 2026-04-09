using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class SettingsPage : ContentPage
{
    private readonly TourApiService _apiService;
    private const string DemoUserId = "demo-user";
    
    // Biến lưu trữ dữ liệu User để giao diện lấy ra hiển thị
    private UserProfile _currentUser = new() { FullName = "User", Email = "user@example.com", AvatarUrl = "" };
    public UserProfile CurrentUser 
    { 
        get => _currentUser; 
        set { _currentUser = value; OnPropertyChanged(); } 
    }

    private string _currentLanguageDisplay = "Tiếng Việt";
    public string CurrentLanguageDisplay
    {
        get => _currentLanguageDisplay;
        set { _currentLanguageDisplay = value; OnPropertyChanged(); }
    }

    private string _premiumTitleText = "Nâng cấp Premium";
    public string PremiumTitleText
    {
        get => _premiumTitleText;
        set { _premiumTitleText = value; OnPropertyChanged(); }
    }

    private string _premiumDescriptionText = "Mở toàn bộ thuyết minh audio, heatmap nâng cao và không quảng cáo.";
    public string PremiumDescriptionText
    {
        get => _premiumDescriptionText;
        set { _premiumDescriptionText = value; OnPropertyChanged(); }
    }

    private string _premiumButtonText = "Mở trang nâng cấp";
    public string PremiumButtonText
    {
        get => _premiumButtonText;
        set { _premiumButtonText = value; OnPropertyChanged(); }
    }

    public SettingsPage()
    {
        InitializeComponent();
        _apiService = new TourApiService();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await RefreshSettingsStateAsync();
    }

    private async Task RefreshSettingsStateAsync()
    {
        // Profile
        var user = await _apiService.GetUserProfileAsync();
        if (user != null)
        {
            CurrentUser = user;
        }

        // Language (không cần restart app, vào lại trang là cập nhật)
        var languageCode = Preferences.Get("SelectedLanguage", "vi");
        CurrentLanguageDisplay = languageCode switch
        {
            "en" => "English",
            "zh" => "中文",
            "ja" => "日本語",
            "ko" => "한국어",
            "fr" => "Français",
            _ => "Tiếng Việt"
        };

        // Premium status (không cần reload app, OnAppearing tự refresh)
        var status = await _apiService.GetPremiumStatusAsync(DemoUserId);
        if (status.IsPremium)
        {
            PremiumTitleText = "Bạn đang dùng Premium";
            PremiumDescriptionText = $"Gói {status.PlanId} còn hạn đến {status.PremiumUntil:dd/MM/yyyy}.";
            PremiumButtonText = "Quản lý gói";
        }
        else
        {
            PremiumTitleText = "Nâng cấp Premium";
            PremiumDescriptionText = "Mở toàn bộ thuyết minh audio, heatmap nâng cao và không quảng cáo.";
            PremiumButtonText = "Mở trang nâng cấp";
        }
    }

    private async void OnUpgradeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UpgradePage());
    }
}