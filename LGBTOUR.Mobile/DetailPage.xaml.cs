using LGBTOUR.Mobile.Models;
using Microsoft.Maui.Media;

namespace LGBTOUR.Mobile;

public partial class DetailPage : ContentPage
{
    public Place SelectedPlace { get; set; }
    
    // Biến dùng để dừng âm thanh ngay lập tức
    private CancellationTokenSource _cts;

    public DetailPage(Place place)
    {
        InitializeComponent();
        SelectedPlace = place;
        BindingContext = this; 

        // Khởi tạo danh sách ngôn ngữ cho Picker
        LanguagePicker.ItemsSource = new List<string> { "🇻🇳 Tiếng Việt", "🇺🇸 English" };
        LanguagePicker.SelectedIndex = 0; // Mặc định chọn Tiếng Việt
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        // Đổi giao diện: Ẩn nút Nghe, Hiện nút Dừng
        PlayButton.IsVisible = false;
        StopButton.IsVisible = true;

        _cts = new CancellationTokenSource();
        string textToRead = SelectedPlace.TtsScript;
        string selectedLang = LanguagePicker.SelectedItem as string;

        // TÍNH NĂNG MOCK TRANSLATION CHO DEMO
        // Nếu user chọn tiếng Anh, ta tự động đổi kịch bản sang tiếng Anh để máy đọc chuẩn giọng
        if (selectedLang != null && selectedLang.Contains("English"))
        {
            if (SelectedPlace.Name.Contains("Dinh"))
                textToRead = "Welcome to Independence Palace, a historic architectural landmark of the city.";
            else if (SelectedPlace.Name.Contains("Nhà thờ"))
                textToRead = "In front of you is Notre Dame Cathedral, an architectural masterpiece over one hundred and forty years old.";
            else
                textToRead = "We are passing by Ben Thanh Market, the most bustling and famous market in the city.";
        }

        try
        {
            // Ra lệnh cho thiết bị phát âm thanh
            await TextToSpeech.Default.SpeakAsync(textToRead, cancelToken: _cts.Token);
        }
        catch (Exception)
        {
            // Bỏ qua lỗi nếu người dùng bấm nút Dừng giữa chừng
        }
        finally
        {
            // Khi đọc xong, trả giao diện về như cũ
            PlayButton.IsVisible = true;
            StopButton.IsVisible = false;
        }
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        // Hủy lệnh đọc ngay lập tức
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    // Nếu người dùng đang nghe mà bấm nút Quay lại (Back), tự động tắt tiếng
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }
}