namespace SaigonAudioTour.Mobile;

public partial class RegisterPage : ContentPage
{
    private const string IsLoggedInKey = "IsLoggedIn";
    private readonly Services.AuthApiService _apiService;

    public RegisterPage()
    {
        InitializeComponent();
        _apiService = IPlatformApplication.Current?.Services.GetService<Services.AuthApiService>()
            ?? throw new InvalidOperationException("AuthApiService chưa được đăng ký DI.");
    }

    // Hàm khi user bấm nút Đăng ký
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var fullName = (FullNameEntry.Text ?? string.Empty).Trim();
        var email = (EmailEntry.Text ?? string.Empty).Trim();
        var password = PasswordEntry.Text ?? string.Empty;
        var confirm = ConfirmPasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Thiếu thông tin", "Vui lòng điền đầy đủ thông tin đăng ký.", "OK");
            return;
        }

        if (!string.Equals(password, confirm, StringComparison.Ordinal))
        {
            await DisplayAlertAsync("Mật khẩu không khớp", "Vui lòng xác nhận mật khẩu chính xác.", "OK");
            return;
        }

        var created = await _apiService.RegisterAsync(fullName, email, password);
        if (created == null)
        {
            await DisplayAlertAsync("Đăng ký thất bại", "Email đã tồn tại hoặc dữ liệu không hợp lệ.", "OK");
            return;
        }

        Preferences.Set(IsLoggedInKey, true);
        Preferences.Set(Services.StorageKeys.UserEmail, created.Email);
        Preferences.Set(Services.StorageKeys.UserFullName, created.FullName);
        Preferences.Set(Services.StorageKeys.UserId, created.UserId.ToString());
        Preferences.Set(Services.StorageKeys.AuthToken, created.Token);

        await DisplayAlertAsync("Thông báo", "Tạo tài khoản thành công!", "OK");
        App.SetRootPage(new AppShell());
    }

    // Hàm khi user bấm chữ "Đăng nhập ngay"
    private async void OnLoginLabelTapped(object sender, EventArgs e)
    {
        // Chuyển sang trang Đăng nhập
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}