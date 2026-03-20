namespace LGBTOUR.Mobile;

public partial class SplashScreen : ContentPage
{
    public SplashScreen()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Giả lập thời gian tải dữ liệu (2 giây)
        await Task.Delay(2000);

        // 2. Chuyển sang trang Đăng nhập bằng hiệu ứng trượt
        await Navigation.PushAsync(new LoginPage());
        
        // 3. Xóa trang Splash khỏi lịch sử chuyển trang để user không bấm Back lại được
        Navigation.RemovePage(this);
    }
}