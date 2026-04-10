using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class LoginPage : ContentPage
{
    private const string IsLoggedInKey = "IsLoggedIn";
    private const string UserEmailKey = "UserEmail";
    private const string UserFullNameKey = "UserFullName";

    public LoginPage()
    {
        InitializeComponent();
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

        var fullName = email.Contains('@') ? email.Split('@')[0] : email;

        Preferences.Set(IsLoggedInKey, true);
        Preferences.Set(UserEmailKey, email);
        Preferences.Set(UserFullNameKey, fullName);

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