namespace LGBTOUR.Mobile;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnLoginClicked(object sender, EventArgs e)
    {
        // Khi gọi API thành công, ta sẽ tráo màn hình Đăng nhập thành màn hình AppShell (Trang chủ)
        var mainPage = Handler.MauiContext.Services.GetService<MainPage>();
        Application.Current.MainPage = new NavigationPage(mainPage);
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        // Mở trang Đăng ký đè lên trên
        await Navigation.PushAsync(new RegisterPage());
    }

    private void OnSkipTapped(object sender, EventArgs e)
    {
        // Cho phép du khách vãng lai bay thẳng vào Trang chủ mà không cần tài khoản
        var mainPage = Handler.MauiContext.Services.GetService<MainPage>();
        Application.Current.MainPage = new NavigationPage(mainPage);
    }
}