using LGBTOUR.Mobile.Services;

namespace LGBTOUR.Mobile;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new AppShell();
    }

    private async void OnRegisterLabelTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    // Hàm mới: Xử lý khi user bấm nút "Để sau"
    private void OnSkipClicked(object sender, EventArgs e)
    {
        // Vào thẳng màn hình chính không cần đăng nhập
        Application.Current.MainPage = new AppShell();
    }
}