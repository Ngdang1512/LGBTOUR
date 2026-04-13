using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class LoginPage : ContentPage
{
    private const string IsLoggedInKey = "IsLoggedIn";
    private const string UserEmailKey = "UserEmail";
    private const string UserFullNameKey = "UserFullName";
    private const string UserIdKey = "UserId";
    private const string AuthTokenKey = "AuthToken";
    private readonly TourApiService _apiService;

    public LoginPage()
    {
        InitializeComponent();
        _apiService = IPlatformApplication.Current?.Services.GetService<TourApiService>() ?? new TourApiService();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = (EmailEntry.Text ?? string.Empty).Trim();
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Thiếu thông tin", "Vui lòng nhập email và mật khẩu.", "OK");
            return;
        }

        var auth = await _apiService.LoginAsync(email, password);
        if (auth == null)
        {
            await DisplayAlertAsync("Đăng nhập thất bại", "Sai email hoặc mật khẩu.", "OK");
            return;
        }

        Preferences.Set(IsLoggedInKey, true);
        Preferences.Set(UserEmailKey, auth.Email);
        Preferences.Set(UserFullNameKey, auth.FullName);
        Preferences.Set(UserIdKey, auth.UserId.ToString());
        Preferences.Set(AuthTokenKey, auth.Token);

        App.SetRootPage(new AppShell());
    }

    private async void OnRegisterLabelTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    // Hàm mới: Xử lý khi user bấm nút "Để sau"
    private void OnSkipClicked(object sender, EventArgs e)
    {
        // Vào thẳng màn hình chính không cần đăng nhập
        App.SetRootPage(new AppShell());
    }
}