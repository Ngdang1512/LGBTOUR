using System.Collections.ObjectModel;
using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services;

namespace LGBTOUR.Mobile;

public partial class MainPage : ContentPage
{
    // Danh sách gốc chứa TẤT CẢ dữ liệu tải về từ API (sẽ không bị thay đổi)
    private List<Place> _allPlaces = new();

    // Danh sách đang hiển thị trên màn hình (sẽ thay đổi liên tục khi lọc/tìm kiếm)
    public ObservableCollection<Place> DisplayPlaces { get; set; } = new();

    private readonly TourApiService _apiService;

    // Biến lưu trữ từ khóa tìm kiếm (Real-time Search)
    private string _searchKeyword = string.Empty;

    // Biến lưu trữ danh mục đang chọn (Tô màu danh mục)
    private string _selectedCategory = "Popular";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    public MainPage()
    {
        InitializeComponent();
        _apiService = new TourApiService();
        BindingContext = this;
    }

    // 1. TỰ ĐỘNG GỌI API KHI MỞ TRANG CHỦ
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Kéo dữ liệu thực tế từ Database về
        _allPlaces = await _apiService.GetAllPlacesAsync();
        
        // Chạy hàm lọc lần đầu tiên để hiển thị dữ liệu
        FilterAndDisplayPlaces();
    }

    // 2. XỬ LÝ KHI BẤM VÀO DANH MỤC (Phổ biến, Ẩm thực, Mua sắm)
    private void OnCategoryTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer gesture)
        {
            if (gesture.CommandParameter is string category)
            {
                SelectedCategory = category;
                
                // Gọi lại hàm lọc mỗi khi đổi danh mục
                FilterAndDisplayPlaces();
            }
        }
    }

    // 3. XỬ LÝ KHI GÕ CHỮ VÀO THANH TÌM KIẾM
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Ghi nhận từ khóa (chuyển thành chữ thường để dễ so sánh)
        _searchKeyword = e.NewTextValue?.ToLower() ?? string.Empty;
        
        // Gọi lại hàm lọc mỗi khi gõ thêm 1 chữ
        FilterAndDisplayPlaces();
    }

    // 4. BỘ NÃO LỌC DỮ LIỆU CHÍNH
    // Hàm này sẽ kết hợp cả 2 điều kiện: Vừa phải đúng Danh mục, Vừa phải chứa Từ khóa tìm kiếm
    private void FilterAndDisplayPlaces()
    {
        // Nếu API chưa trả về dữ liệu hoặc lỗi, thì không làm gì cả
        if (_allPlaces == null || !_allPlaces.Any()) return;

        var filtered = _allPlaces.Where(p => 
            // Điều kiện 1: Phải đúng danh mục đang chọn (Popular, Food, Shopping)
            p.Category == SelectedCategory && 
            
            // Điều kiện 2 (Mới nâng cấp): Từ khóa tìm kiếm phải rỗng, 
            // HOẶC có chứa trong Tên điểm đến (Name)
            // HOẶC có chứa trong Địa điểm (Location)
            (string.IsNullOrWhiteSpace(_searchKeyword) || 
             p.Name.ToLower().Contains(_searchKeyword) || 
             p.Location.ToLower().Contains(_searchKeyword))
        ).ToList();

        // Xóa danh sách cũ và cập nhật danh sách mới đã lọc lên màn hình
        DisplayPlaces.Clear();
        foreach (var p in filtered)
        {
            DisplayPlaces.Add(p);
        }
    }

    private async void OnCardTapped(object sender, EventArgs e)
    {
        // 1. Lấy thông tin địa điểm mà người dùng vừa bấm vào
        var border = (Border)sender;
        var selectedPlace = (Place)border.BindingContext;

        // 2. Đóng gói dữ liệu vào một kiện hàng tên là "placeInfo"
        var navigationParameter = new Dictionary<string, object>
        {
            { "placeInfo", selectedPlace }
        };

        // 3. Chuyển trang và xách theo kiện hàng đó
        await Shell.Current.GoToAsync("DetailPage", navigationParameter);
    }
}