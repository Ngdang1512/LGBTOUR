using Microsoft.Maui.Media; // Thư viện đọc văn bản tích hợp sẵn
using LGBTOUR.Mobile.Models;

namespace LGBTOUR.Mobile;

// Khai báo để trang này biết cách nhận dữ liệu Place được truyền sang
[QueryProperty(nameof(SelectedPlace), "placeInfo")]
public partial class DetailPage : ContentPage
{
    private Place _selectedPlace;
    public Place SelectedPlace
    {
        get => _selectedPlace;
        set { _selectedPlace = value; OnPropertyChanged(); }
    }

    private bool _isPlaying = false;
    private CancellationTokenSource _cts;

    public DetailPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Xử lý khi bấm nút Quay lại
    private async void OnBackTapped(object sender, EventArgs e)
    {
        StopAudio(); // Tắt tiếng ngay lập tức nếu đang đọc dở mà thoát trang
        await Shell.Current.GoToAsync("..");
    }

    // Xử lý tính năng Đọc Thuyết minh (Text-To-Speech)
    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        // 1. Nếu đang phát thì bấm vào sẽ Dừng lại
        if (_isPlaying)
        {
            StopAudio();
            return;
        }

        // 2. Kiểm tra xem địa điểm này có kịch bản chưa
        if (SelectedPlace == null || string.IsNullOrWhiteSpace(SelectedPlace.TtsScript))
        {
            await DisplayAlert("Thông báo", "Chưa có kịch bản thuyết minh cho địa điểm này.", "OK");
            return;
        }

        // 3. Bắt đầu phát âm thanh
        _isPlaying = true;
        PlayButton.Text = "⏹ ĐANG PHÁT... (BẤM ĐỂ DỪNG)";
        PlayButton.BackgroundColor = Color.FromArgb("#EF4444"); // Đổi nút sang màu đỏ

        _cts = new CancellationTokenSource();

        try
        {
            // Lệnh gọi hệ thống tự động đọc văn bản
            await TextToSpeech.Default.SpeakAsync(SelectedPlace.TtsScript, cancelToken: _cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi đọc văn bản: {ex.Message}");
        }
        finally
        {
            // Khi đọc xong (hoặc bị hủy), trả nút về trạng thái cũ
            _isPlaying = false;
            PlayButton.Text = "▶️ NGHE THUYẾT MINH";
            PlayButton.BackgroundColor = Color.FromArgb("#4F46E5");
        }
    }

    private void StopAudio()
    {
        if (_cts?.IsCancellationRequested == false)
        {
            _cts.Cancel(); // Ra lệnh ngừng đọc
        }
    }
}