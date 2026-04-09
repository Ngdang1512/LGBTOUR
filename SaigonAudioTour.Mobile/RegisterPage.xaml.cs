namespace SaigonAudioTour.Mobile;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    // Hàm khi user bấm nút Đăng ký
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Giả lập đăng ký thành công cho Demo
        await DisplayAlertAsync("Thông báo", "Tạo tài khoản thành công!", "OK");
        
        // Quay về trang Đăng nhập
        await Navigation.PopAsync();
    }

    // Hàm khi user bấm chữ "Đăng nhập ngay"
    private async void OnLoginLabelTapped(object sender, EventArgs e)
    {
        // Quay về trang Đăng nhập
        await Navigation.PopAsync();
    }
}