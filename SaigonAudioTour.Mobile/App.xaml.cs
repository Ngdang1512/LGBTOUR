namespace SaigonAudioTour.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Services.AppLanguageService.ApplyCurrentAppLanguage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Bắt đầu bằng splash động rồi chuyển sang Shell
        return new Window(new AnimatedSplashPage());
    }

    public static void SetRootPage(Page rootPage)
    {
        if (Current?.Windows.Count > 0)
        {
            Current.Windows[0].Page = rootPage;
        }
        else if (Current != null)
        {
            Current.OpenWindow(new Window(rootPage));
        }
    }
}
