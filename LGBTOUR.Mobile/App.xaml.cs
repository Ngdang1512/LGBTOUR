namespace LGBTOUR.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Khởi động vào trang Splash có Animation trước
        MainPage = new AnimatedSplashPage();
    }
}