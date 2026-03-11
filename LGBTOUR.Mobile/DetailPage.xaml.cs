namespace LGBTOUR.Mobile;

public partial class DetailPage : ContentPage
{
    public DetailPage()
    {
        InitializeComponent();
    }

    // Hàm xử lý khi bấm nút Back
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(".."); // Lệnh ".." nghĩa là quay lại trang trước đó
    }
}