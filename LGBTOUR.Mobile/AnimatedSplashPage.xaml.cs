namespace LGBTOUR.Mobile;

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
            // === GIAI ĐOẠN 1: KHỞI ĐỘNG (Food Icon spin & fade) ===
            FoodIcon.Opacity = 0;
            FoodIcon.Rotation = 0;
            
            await Task.WhenAll(
                FoodIcon.FadeToAsync(1, 600, Easing.SinOut),
                FoodIcon.RotateToAsync(360, 1000, Easing.SinInOut)
            );

            // === GIAI ĐOẠN 2: BRAND LABEL (Slide up & fade) ===
            BrandLabel.TranslationY = 20;
            BrandLabel.Opacity = 0;
            SubtitleLabel.TranslationY = 20;
            SubtitleLabel.Opacity = 0;
            
            await Task.WhenAll(
                BrandLabel.FadeToAsync(1, 500, Easing.SinOut),
                BrandLabel.TranslateToAsync(0, 0, 500, Easing.CubicOut),
                SubtitleLabel.FadeToAsync(1, 300, Easing.SinOut)
            );

            // === GIAI ĐOẠN 3: SLOGAN & LOADING ===
            SloganLabel.TranslationY = 20;
            SloganLabel.Opacity = 0;
            LoadingLabel.Opacity = 0;
            
            await Task.WhenAll(
                SloganLabel.FadeToAsync(1, 400, Easing.SinOut),
                SloganLabel.TranslateToAsync(0, 0, 400, Easing.CubicOut),
                LoadingLabel.FadeToAsync(1, 400, Easing.SinOut)
            );

            // === GIAI ĐOẠN 4: PROGRESS BAR (Loading animation) ===
            await ProgressIndicator.ProgressTo(1.0, 1500, Easing.SinInOut);

            // === GIAI ĐOẠN 5: CHUYỂN HƯỚNG ===
            await Task.Delay(500);
            
            // Chuyển đến trang chủ Shell
            if (Application.Current != null)
                Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AnimatedSplashPage: {ex.Message}");
            // Nếu animation thất bại, vẫn chuyển đến Shell
            if (Application.Current != null)
                Application.Current.MainPage = new AppShell();
        }
    }
}