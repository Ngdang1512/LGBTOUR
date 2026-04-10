using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class SettingsPage : ContentPage
{
    private readonly TourApiService _apiService;
    private const string DemoUserId = "demo-user";
    private const string SelectedLanguageKey = "SelectedLanguage";
    private const string IsLoggedInKey = "IsLoggedIn";
    private const string UserEmailKey = "UserEmail";
    private const string UserFullNameKey = "UserFullName";
    
    // Biến lưu trữ dữ liệu User để giao diện lấy ra hiển thị
    private UserProfile _currentUser = new() { FullName = "User", Email = "user@example.com", AvatarUrl = "" };
    public UserProfile CurrentUser 
    { 
        get => _currentUser; 
        set { _currentUser = value; OnPropertyChanged(); } 
    }

    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowLoginActions));
        }
    }

    public bool ShowLoginActions => !IsLoggedIn;

    private bool _isPremium;
    public bool IsPremium
    {
        get => _isPremium;
        set
        {
            _isPremium = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PremiumCardBackgroundColor));
            OnPropertyChanged(nameof(PremiumCardStrokeColor));
            OnPropertyChanged(nameof(PremiumTitleColor));
            OnPropertyChanged(nameof(PremiumDescriptionColor));
            OnPropertyChanged(nameof(PremiumBenefitColor));
            OnPropertyChanged(nameof(PremiumButtonBackgroundColor));
            OnPropertyChanged(nameof(PremiumButtonTextColor));
        }
    }

    public string PremiumCardBackgroundColor => IsPremium ? "#ECFDF5" : "#FFF7ED";
    public string PremiumCardStrokeColor => IsPremium ? "#86EFAC" : "#FDBA74";
    public string PremiumTitleColor => IsPremium ? "#166534" : "#9A3412";
    public string PremiumDescriptionColor => IsPremium ? "#14532D" : "#7C2D12";
    public string PremiumBenefitColor => IsPremium ? "#15803D" : "#7C2D12";
    public string PremiumButtonBackgroundColor => IsPremium ? "#16A34A" : "#F97316";
    public string PremiumButtonTextColor => "White";

    private string _accountTitleText = "Tài khoản";
    public string AccountTitleText
    {
        get => _accountTitleText;
        set { _accountTitleText = value; OnPropertyChanged(); }
    }

    private string _editLabelText = "Sửa";
    public string EditLabelText
    {
        get => _editLabelText;
        set { _editLabelText = value; OnPropertyChanged(); }
    }

    private string _loginPromptText = "Đăng nhập để đồng bộ lịch sử tour và gói Premium";
    public string LoginPromptText
    {
        get => _loginPromptText;
        set { _loginPromptText = value; OnPropertyChanged(); }
    }

    private string _loginButtonText = "Đăng nhập";
    public string LoginButtonText
    {
        get => _loginButtonText;
        set { _loginButtonText = value; OnPropertyChanged(); }
    }

    private string _registerButtonText = "Đăng ký";
    public string RegisterButtonText
    {
        get => _registerButtonText;
        set { _registerButtonText = value; OnPropertyChanged(); }
    }

    private string _serviceSectionText = "GÓI DỊCH VỤ";
    public string ServiceSectionText
    {
        get => _serviceSectionText;
        set { _serviceSectionText = value; OnPropertyChanged(); }
    }

    private string _generalSectionText = "CÀI ĐẶT CHUNG";
    public string GeneralSectionText
    {
        get => _generalSectionText;
        set { _generalSectionText = value; OnPropertyChanged(); }
    }

    private string _languageLabelText = "Ngôn ngữ";
    public string LanguageLabelText
    {
        get => _languageLabelText;
        set { _languageLabelText = value; OnPropertyChanged(); }
    }

    private string _notificationLabelText = "Thông báo";
    public string NotificationLabelText
    {
        get => _notificationLabelText;
        set { _notificationLabelText = value; OnPropertyChanged(); }
    }

    private string _supportSectionText = "HỖ TRỢ";
    public string SupportSectionText
    {
        get => _supportSectionText;
        set { _supportSectionText = value; OnPropertyChanged(); }
    }

    private string _helpCenterLabelText = "Trung tâm trợ giúp";
    public string HelpCenterLabelText
    {
        get => _helpCenterLabelText;
        set { _helpCenterLabelText = value; OnPropertyChanged(); }
    }

    private string _logoutButtonText = "Đăng xuất";
    public string LogoutButtonText
    {
        get => _logoutButtonText;
        set { _logoutButtonText = value; OnPropertyChanged(); }
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

    private string _premiumBenefitText = "";
    public string PremiumBenefitText
    {
        get => _premiumBenefitText;
        set { _premiumBenefitText = value; OnPropertyChanged(); }
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
        var languageCode = Preferences.Get(SelectedLanguageKey, "vi");
        ApplyLocalizedTexts(languageCode);

        IsLoggedIn = Preferences.Get(IsLoggedInKey, false);

        // Profile
        if (IsLoggedIn)
        {
            var savedEmail = Preferences.Get(UserEmailKey, "");
            var savedName = Preferences.Get(UserFullNameKey, "");

            var user = await _apiService.GetUserProfileAsync();
            if (user != null)
            {
                CurrentUser = user;
            }
            else
            {
                CurrentUser = new UserProfile
                {
                    FullName = string.IsNullOrWhiteSpace(savedName) ? (languageCode == "vi" ? "Người dùng" : "User") : savedName,
                    Email = string.IsNullOrWhiteSpace(savedEmail) ? "user@example.com" : savedEmail,
                    AvatarUrl = ""
                };
            }
        }
        else
        {
            CurrentUser = new UserProfile
            {
                FullName = languageCode == "vi" ? "Khách" : "Guest",
                Email = languageCode == "vi" ? "Bạn chưa đăng nhập" : "Not signed in",
                AvatarUrl = ""
            };
        }

        // Language
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
            IsPremium = true;
            if (languageCode == "vi")
            {
                PremiumTitleText = "Bạn đang dùng Premium 👑";
                PremiumDescriptionText = $"Gói {status.PlanId} còn hạn đến {status.PremiumUntil:dd/MM/yyyy}.";
                PremiumBenefitText = "Đã mở khóa: toàn bộ audio, trải nghiệm không quảng cáo, quyền lợi ưu tiên.";
                PremiumButtonText = "Xem gói Premium";
            }
            else
            {
                PremiumTitleText = "You're on Premium 👑";
                PremiumDescriptionText = $"Plan {status.PlanId} is active until {status.PremiumUntil:dd/MM/yyyy}.";
                PremiumBenefitText = "Unlocked: full audio, ad-free experience, and priority benefits.";
                PremiumButtonText = "View Premium plans";
            }
        }
        else
        {
            IsPremium = false;
            PremiumTitleText = languageCode == "vi" ? "Nâng cấp Premium" : "Upgrade to Premium";
            PremiumDescriptionText = languageCode == "vi"
                ? "Mở toàn bộ thuyết minh audio, heatmap nâng cao và không quảng cáo."
                : "Unlock full audio guides, advanced heatmap, and an ad-free experience.";
            PremiumBenefitText = string.Empty;
            PremiumButtonText = languageCode == "vi" ? "Mở trang nâng cấp" : "Open upgrade page";
        }
    }

    private async void OnUpgradeClicked(object sender, EventArgs e)
    {
        var languageCode = Preferences.Get(SelectedLanguageKey, "vi");

        if (IsPremium)
        {
            await DisplayAlertAsync(
                "Premium",
                languageCode == "vi"
                    ? "Bạn đang dùng Premium. Có thể mua thêm tại trang quản lý gói nếu muốn gia hạn."
                    : "You are already on Premium. You can still purchase another plan to extend it.",
                "OK");
            await Navigation.PushAsync(new UpgradePage());
            return;
        }

        await Navigation.PushAsync(new UpgradePage());
    }

    private async void OnLanguageTapped(object sender, TappedEventArgs e)
    {
        var currentCode = Preferences.Get(SelectedLanguageKey, "vi");
        var isVi = currentCode == "vi";

        var selected = await DisplayActionSheetAsync(
            isVi ? "Chọn ngôn ngữ" : "Choose language",
            isVi ? "Hủy" : "Cancel",
            null,
            "Tiếng Việt",
            "English",
            "中文",
            "日本語",
            "한국어",
            "Français");

        if (string.IsNullOrWhiteSpace(selected) || selected == "Hủy" || selected == "Cancel")
        {
            return;
        }

        var languageCode = selected switch
        {
            "English" => "en",
            "中文" => "zh",
            "日本語" => "ja",
            "한국어" => "ko",
            "Français" => "fr",
            _ => "vi"
        };

        if (string.Equals(currentCode, languageCode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Preferences.Set(SelectedLanguageKey, languageCode);
        await RefreshSettingsStateAsync();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Preferences.Set(IsLoggedInKey, false);
        Preferences.Remove(UserEmailKey);
        Preferences.Remove(UserFullNameKey);

        await RefreshSettingsStateAsync();
        var languageCode = Preferences.Get(SelectedLanguageKey, "vi");
        await DisplayAlertAsync(
            languageCode == "vi" ? "Thông báo" : "Notice",
            languageCode == "vi" ? "Bạn đã đăng xuất." : "You have signed out.",
            "OK");
    }

    private void ApplyLocalizedTexts(string languageCode)
    {
        var isVi = languageCode == "vi";

        AccountTitleText = isVi ? "Tài khoản" : "Account";
        EditLabelText = isVi ? "Sửa" : "Edit";
        LoginPromptText = isVi
            ? "Đăng nhập để đồng bộ lịch sử tour và gói Premium"
            : "Sign in to sync tour history and your Premium plan";
        LoginButtonText = isVi ? "Đăng nhập" : "Sign in";
        RegisterButtonText = isVi ? "Đăng ký" : "Register";
        ServiceSectionText = isVi ? "GÓI DỊCH VỤ" : "SERVICE PLAN";
        GeneralSectionText = isVi ? "CÀI ĐẶT CHUNG" : "GENERAL SETTINGS";
        LanguageLabelText = isVi ? "Ngôn ngữ" : "Language";
        NotificationLabelText = isVi ? "Thông báo" : "Notifications";
        SupportSectionText = isVi ? "HỖ TRỢ" : "SUPPORT";
        HelpCenterLabelText = isVi ? "Trung tâm trợ giúp" : "Help Center";
        LogoutButtonText = isVi ? "Đăng xuất" : "Sign out";
    }
}