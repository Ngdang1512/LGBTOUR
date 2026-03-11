using System.Collections.ObjectModel;
using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services; // Nhúng thư mục Service vào

namespace LGBTOUR.Mobile;

public partial class MapPage : ContentPage
{
    public ObservableCollection<Place> RoutePlaces { get; set; } = new();
    private readonly TourApiService _apiService;

    public MapPage()
    {
        InitializeComponent();
        
        // Khởi tạo dịch vụ API
        _apiService = new TourApiService();
        
        BindingContext = this;
    }

    // Hàm này sẽ tự động chạy mỗi khi người dùng mở sang tab Khám phá
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Gọi dịch vụ để đi lấy dữ liệu từ API của người bạn kia
        var placesFromDatabase = await _apiService.GetRoutePlacesAsync();

        // 2. Nếu lấy được dữ liệu, đưa nó lên màn hình
        if (placesFromDatabase.Any())
        {
            RoutePlaces.Clear();
            foreach (var place in placesFromDatabase)
            {
                RoutePlaces.Add(place);
            }
        }
    }
}