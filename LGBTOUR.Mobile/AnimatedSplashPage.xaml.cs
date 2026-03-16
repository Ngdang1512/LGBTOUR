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

        // --- GIAI ĐOẠN 1: CHUẨN BỊ CẤT CÁNH ---
        // Giấu máy bay xuống góc dưới bên trái, thu nhỏ lại và chúi mũi lên trên (góc -45 độ)
        AirplaneIcon.TranslationX = -200;
        AirplaneIcon.TranslationY = 200;
        AirplaneIcon.Rotation = -45;
        AirplaneIcon.Scale = 0.5;

        // --- GIAI ĐOẠN 2: BAY LÊN ---
        // Cho máy bay hiện ra thật nhanh (0.3 giây)
        _ = AirplaneIcon.FadeTo(1, 300);

        // Kích hoạt 3 hành động cùng lúc trong 1.5 giây: 
        // Bay vào giữa (Translate) + Thẳng lái lại (Rotate) + Phóng to ra (Scale)
        await Task.WhenAll(
            AirplaneIcon.TranslateTo(0, 0, 1500, Easing.CubicOut), 
            AirplaneIcon.RotateTo(0, 1500, Easing.SpringOut), // SpringOut tạo độ rung nhẹ như phi cơ cản gió
            AirplaneIcon.ScaleTo(1, 1500, Easing.CubicOut)
        );

        // --- GIAI ĐOẠN 3: HIỆN CHỮ ---
        BrandLabel.TranslationY = 15; 
        await Task.WhenAll(
            BrandLabel.FadeTo(1, 600, Easing.CubicOut),
            BrandLabel.TranslateTo(0, 0, 600, Easing.CubicOut)
        );

        await SloganLabel.FadeTo(1, 600, Easing.Linear);

        // Đợi 1.5 giây để du khách ngắm nghía
        await Task.Delay(1500);

        // --- GIAI ĐOẠN 4: VÀO APP ---
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }
}