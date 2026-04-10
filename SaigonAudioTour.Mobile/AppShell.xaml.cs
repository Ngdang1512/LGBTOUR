namespace SaigonAudioTour.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute("DetailPage", typeof(DetailPage));
        Routing.RegisterRoute(nameof(UpgradePage), typeof(UpgradePage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
    }
}