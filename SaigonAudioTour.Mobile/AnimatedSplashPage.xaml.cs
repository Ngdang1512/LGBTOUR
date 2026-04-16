namespace SaigonAudioTour.Mobile;

public partial class AnimatedSplashPage : ContentPage
{
    private bool _hasAnimated;
    private const string FullBrandText = "Saigon Audio Tour";

    public AnimatedSplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_hasAnimated) return;
        _hasAnimated = true;

        try
        {
            // === GIAI ĐOẠN 1: Badge xuất hiện mượt ===
            Badge.Opacity = 0;
            Badge.Scale = 0.78;

            SatLabel.Opacity = 0;
            SatLabel.TranslationX = 0;
            SatLabel.TranslationY = 14;
            SatLabel.Scale = 1;

            FullBrandLabel.Opacity = 0;
            FullBrandLabel.TranslationY = 6;
            FullBrandLabel.Scale = 0.92;

            SubtitleLabel.Opacity = 0;
            SubtitleLabel.TranslationY = 14;

            await Task.WhenAll(
                Badge.FadeToAsync(1, 700, Easing.CubicOut),
                Badge.ScaleToAsync(1, 700, Easing.CubicOut)
            );

            // === GIAI ĐOẠN 2: SAT + subtitle hiện lên nhẹ nhàng ===
            await Task.WhenAll(
                SatLabel.FadeToAsync(1, 620, Easing.CubicOut),
                SatLabel.TranslateToAsync(0, 0, 620, Easing.CubicOut),
                SubtitleLabel.FadeToAsync(1, 620, Easing.CubicOut),
                SubtitleLabel.TranslateToAsync(0, 0, 620, Easing.CubicOut)
            );

            await Task.Delay(280);

            // === GIAI ĐOẠN 3: SAT dissolve + zoom thành Saigon Audio Tour (không trượt ngang) ===
            await Task.WhenAll(
                Badge.ScaleToAsync(1.03, 460, Easing.SinInOut),
                Badge.FadeToAsync(0.98, 460, Easing.SinInOut)
            );

            FullBrandLabel.Text = string.Empty;

            await Task.WhenAll(
                SatLabel.ScaleToAsync(1.16, 1120, Easing.SinInOut),
                SatLabel.TranslateToAsync(0, -8, 1120, Easing.SinInOut),
                SatLabel.FadeToAsync(0, 920, Easing.SinInOut),
                FullBrandLabel.ScaleToAsync(1, 1120, Easing.SinInOut),
                FullBrandLabel.TranslateToAsync(0, 0, 1120, Easing.SinInOut),
                FullBrandLabel.FadeToAsync(1, 520, Easing.CubicOut)
            );

            await TypeTextAsync(FullBrandLabel, FullBrandText, 48);

            await Badge.ScaleToAsync(1, 340, Easing.CubicOut);

            // === GIAI ĐOẠN 4: SLOGAN & LOADING ===
            SloganLabel.TranslationY = 20;
            SloganLabel.Opacity = 0;
            LoadingLabel.Opacity = 0;
            
            await Task.WhenAll(
                SloganLabel.FadeToAsync(1, 560, Easing.CubicOut),
                SloganLabel.TranslateToAsync(0, 0, 560, Easing.CubicOut),
                LoadingLabel.FadeToAsync(1, 560, Easing.CubicOut)
            );

            // === GIAI ĐOẠN 5: PROGRESS BAR (Loading animation) ===
            await ProgressIndicator.ProgressTo(1.0, 1900, Easing.CubicInOut);

            // === GIAI ĐOẠN 6: CHUYỂN HƯỚNG ===
            await Task.Delay(420);
            
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

    private static async Task TypeTextAsync(Label label, string fullText, int delayMs)
    {
        label.Text = string.Empty;
        for (var i = 1; i <= fullText.Length; i++)
        {
            label.Text = fullText[..i];
            await Task.Delay(delayMs);
        }
    }
}