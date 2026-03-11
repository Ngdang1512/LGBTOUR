using System.Collections.ObjectModel;
using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services;

namespace LGBTOUR.Mobile;

public partial class MainPage : ContentPage
{
    private List<Place> _allPlaces = new();
    public ObservableCollection<Place> DisplayPlaces { get; set; } = new();
    private readonly TourApiService _apiService;

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

    // Tự động gọi API khi mở trang
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Kéo dữ liệu từ Database về
        _allPlaces = await _apiService.GetAllPlacesAsync();
        
        // Lọc dữ liệu lần đầu
        FilterAndDisplayPlaces();
    }

    private void OnCategoryTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer gesture)
        {
            if (gesture.CommandParameter is string category)
            {
                SelectedCategory = category;
                FilterAndDisplayPlaces(); // Gọi hàm lọc
            }
        }
    }

    // Hàm phụ trợ giúp lọc danh sách theo danh mục (Popular, Food, Shopping)
    private void FilterAndDisplayPlaces()
    {
        if (_allPlaces == null || !_allPlaces.Any()) return;

        var filtered = _allPlaces.Where(p => p.Category == SelectedCategory).ToList();
        DisplayPlaces.Clear();
        foreach (var p in filtered)
        {
            DisplayPlaces.Add(p);
        }
    }

    private async void OnCardTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("DetailPage");
    }
}