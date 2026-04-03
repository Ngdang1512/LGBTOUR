using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services;

namespace LGBTOUR.Mobile;

public partial class SettingsPage : ContentPage
{
    private readonly TourApiService _apiService;
    
    // Biến lưu trữ dữ liệu User để giao diện lấy ra hiển thị
    private UserProfile _currentUser = new() { FullName = "User", Email = "user@example.com", AvatarUrl = "" };
    public UserProfile CurrentUser 
    { 
        get => _currentUser; 
        set { _currentUser = value; OnPropertyChanged(); } 
    }

    public SettingsPage()
    {
        InitializeComponent();
        _apiService = new TourApiService();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Gọi API lấy thông tin Profile
        var user = await _apiService.GetUserProfileAsync();
        if (user != null)
        {
            CurrentUser = user;
        }
    }
}