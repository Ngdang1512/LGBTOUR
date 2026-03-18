using System.Collections.ObjectModel;
using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services;

namespace LGBTOUR.Mobile;

public partial class MainPage : ContentPage
{
    // Danh sách gốc chứa TẤT CẢ dữ liệu
    private List<Place> _allPlaces = new();
    
    // Danh sách hiển thị trên màn hình
    public ObservableCollection<Place> DisplayPlaces { get; set; } = new();

    private readonly TourApiService _apiService;
    private string _searchKeyword = string.Empty;
    private string _selectedCategory = "Popular";

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged();
        }
    }

    // Tiêm Service (Dependency Injection) qua Constructor
    public MainPage(TourApiService apiService)
    {
        InitializeComponent();
        
        _apiService = apiService;
        BindingContext = this; // Rất quan trọng để UI nhận được dữ liệu
    }

    // Hàm này tự động chạy SAU KHI giao diện đã hiển thị xong -> Không làm treo app
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Chỉ tải dữ liệu nếu danh sách đang trống
        if (DisplayPlaces.Count == 0)
        {
            await LoadDataAsync();
        }
    }

    // Hàm tải dữ liệu riêng biệt
    private async Task LoadDataAsync()
    {
        try
        {
            // Tạm thời comment code gọi API để test giao diện lên hình trước
            // var places = await _apiService.GetPlacesAsync();
            // ... xử lý đưa places vào DisplayPlaces ...
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể tải dữ liệu: " + ex.Message, "OK");
        }
    }
}