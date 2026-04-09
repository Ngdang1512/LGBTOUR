namespace SaigonAudioTour.Mobile;

public partial class AnimatedSplashPage : ContentPage
{
    public AnimatedSplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // === GIAI ĐOẠN 1: SAT badge xuất hiện ===
            Badge.Opacity = 0;
            Badge.Scale = 0.82;

            await Task.WhenAll(
                Badge.FadeToAsync(1, 450, Easing.SinOut),
                Badge.ScaleToAsync(1, 450, Easing.SinOut)
            );

            // === GIAI ĐOẠN 2: SAT hiện lên ===
            BrandLabel.TranslationY = 20;
            BrandLabel.Opacity = 0;
            SubtitleLabel.TranslationY = 20;
            SubtitleLabel.Opacity = 0;
            
            await Task.WhenAll(
                BrandLabel.FadeToAsync(1, 500, Easing.SinOut),
                BrandLabel.TranslateToAsync(0, 0, 500, Easing.CubicOut),
                SubtitleLabel.FadeToAsync(1, 300, Easing.SinOut)
            );

            await Task.Delay(250);

            // === GIAI ĐOẠN 3: SAT mở ra thành Saigon Audio Tour ===
            await Task.WhenAll(
                Badge.ScaleToAsync(1.08, 220, Easing.SinOut),
                Badge.FadeToAsync(0.95, 220, Easing.SinOut)
            );

            BrandLabel.Text = "Saigon Audio Tour";
            BrandLabel.FontSize = 30;
            BrandLabel.CharacterSpacing = 1;
            SubtitleLabel.Text = "City tour thuyết minh tự động";
            SubtitleLabel.FontSize = 15;

            await Task.WhenAll(
                BrandLabel.FadeToAsync(1, 300, Easing.SinOut),
                BrandLabel.TranslateToAsync(0, -2, 300, Easing.CubicOut),
                SubtitleLabel.FadeToAsync(1, 300, Easing.SinOut)
            );

            // === GIAI ĐOẠN 4: SLOGAN & LOADING ===
            SloganLabel.TranslationY = 20;
            SloganLabel.Opacity = 0;
            LoadingLabel.Opacity = 0;
            
            await Task.WhenAll(
                SloganLabel.FadeToAsync(1, 400, Easing.SinOut),
                SloganLabel.TranslateToAsync(0, 0, 400, Easing.CubicOut),
                LoadingLabel.FadeToAsync(1, 400, Easing.SinOut)
            );

            // === GIAI ĐOẠN 5: PROGRESS BAR (Loading animation) ===
            await ProgressIndicator.ProgressTo(1.0, 1500, Easing.SinInOut);

            // === GIAI ĐOẠN 6: CHUYỂN HƯỚNG ===
            await Task.Delay(500);
            
            // Chuyển đến trang chủ Shell
            App.SetRootPage(new AppShell());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AnimatedSplashPage: {ex.Message}");
            // Nếu animation thất bại, vẫn chuyển đến Shell
            App.SetRootPage(new AppShell());
        }
    }
}