using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnLoginClicked(object sender, EventArgs e)
    {
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