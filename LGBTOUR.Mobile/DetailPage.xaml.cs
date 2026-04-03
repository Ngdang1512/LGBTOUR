using LGBTOUR.Mobile.Models;
using Microsoft.Maui.Media;

namespace LGBTOUR.Mobile;

public partial class DetailPage : ContentPage
{
    public Place? SelectedPlace { get; set; }
    
    // Biến dùng để dừng âm thanh ngay lập tức
    private CancellationTokenSource _cts = new();

    public DetailPage(Place? place = null)
    {
        InitializeComponent();
        SelectedPlace = place;
        BindingContext = this; 

        // Khởi tạo danh sách ngôn ngữ cho Picker
        LanguagePicker.ItemsSource = new List<string> { 
            "🇻🇳 Tiếng Việt", 
            "🇬🇧 English",
            "🇨🇳 中文",
            "🇯🇵 日本語",
            "🇰🇷 한국어",
            "🇫🇷 Français"
        };
        LanguagePicker.SelectedIndex = 0; // Mặc định chọn Tiếng Việt
    }

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        // Đổi giao diện: Ẩn nút Phát, Hiện nút Dừng
        PlayButton.IsVisible = false;
        StopButton.IsVisible = true;

        _cts = new CancellationTokenSource();
        string textToRead = SelectedPlace.TtsScript;
        string selectedLang = LanguagePicker.SelectedItem as string;

        // TÍNH NĂNG MOCK TRANSLATION CHO DEMO
        // Nếu user chọn tiếng khác, ta tự động đổi kịch bản
        if (selectedLang != null)
        {
            if (selectedLang.Contains("English"))
                textToRead = TranslateToEnglish(textToRead);
            else if (selectedLang.Contains("中文"))
                textToRead = TranslateToChinese(textToRead);
            else if (selectedLang.Contains("日本"))
                textToRead = TranslateToJapanese(textToRead);
        }

        try
        {
            // Ra lệnh cho thiết bị phát âm thanh
            await TextToSpeech.Default.SpeakAsync(textToRead, cancelToken: _cts.Token);
            
            // Cập nhật progress bar
            await AnimateProgressBar();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể phát âm thanh: {ex.Message}", "OK");
        }
        finally
        {
            // Khi đọc xong, trả giao diện về như cũ
            PlayButton.IsVisible = true;
            StopButton.IsVisible = false;
            AudioProgressBar.Progress = 0;
        }
    }

    private async Task AnimateProgressBar()
    {
        // Mô phỏng progress bar animation trong 8 giây
        for (int i = 0; i <= 100; i++)
        {
            if (_cts?.IsCancellationRequested == true) break;
            AudioProgressBar.ProgressTo(i / 100.0, 80, Easing.Linear);
            await Task.Delay(80);
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

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnCallClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Gọi Quán", "Chức năng sẽ được cập nhật", "OK");
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        if (SelectedPlace.Latitude > 0 && SelectedPlace.Longitude > 0)
        {
            var location = new Location(SelectedPlace.Latitude, SelectedPlace.Longitude);
            await Map.Default.OpenAsync(location, new MapLaunchOptions { Name = SelectedPlace.Name });
        }
    }

    private async void OnShareClicked(object sender, EventArgs e)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = SelectedPlace.Name,
            Text = $"Khám phá {SelectedPlace.Name} cùng LGBTOUR!\n{SelectedPlace.TtsScript}",
            Uri = "https://lgbtour.app"
        });
    }

    // Hàm dịch sang các ngôn ngữ (Mock)
    private string TranslateToEnglish(string text)
    {
        return $"Welcome to {SelectedPlace.Name}. {SelectedPlace.TtsScript}";
    }

    private string TranslateToChinese(string text)
    {
        return $"欢迎来到{SelectedPlace.Name}。 {SelectedPlace.TtsScript}";
    }

    private string TranslateToJapanese(string text)
    {
        return $"{SelectedPlace.Name}へようこそ。 {SelectedPlace.TtsScript}";
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