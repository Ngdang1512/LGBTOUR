namespace SaigonAudioTour.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute("DetailPage", typeof(DetailPage));
        Routing.RegisterRoute(nameof(UpgradePage), typeof(UpgradePage));
    }
}