namespace LGBTOUR.Mobile;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Sau khi gọi API đăng ký thành công, báo lỗi hoặc quay lại trang đăng nhập
        await DisplayAlert("Thành công", "Tạo tài khoản thành công!", "OK");
        await Navigation.PopAsync();
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        // Quay lại trang đăng nhập
        await Navigation.PopAsync();
    }
}