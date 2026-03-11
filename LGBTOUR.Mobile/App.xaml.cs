namespace LGBTOUR.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Thay vì bọc AppShell, chúng ta bọc LoginPage trong NavigationPage 
        // để khởi chạy luồng xác thực (Authentication Flow) đầu tiên
        MainPage = new NavigationPage(new LoginPage());
    }
}