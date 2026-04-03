namespace LGBTOUR.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Khởi động vào trang LoginPage
        MainPage = new LoginPage();
    }
}
