using System.Collections.ObjectModel;
using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services;

namespace LGBTOUR.Mobile;

public partial class MainPage : ContentPage
{
    private List<Place> _allPlaces = new();
    public ObservableCollection<Place> DisplayPlaces { get; set; } = new();

    private readonly TourApiService _apiService;
    private string _selectedCategory = "Phổ biến";

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterPlaces(); 
            }
        }
    }

    public MainPage(TourApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
        BindingContext = this; 
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_allPlaces.Count == 0)
        {
            await LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var places = await _apiService.GetAllPlacesAsync();
            _allPlaces = places;
            FilterPlaces();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể tải dữ liệu: " + ex.Message, "OK");
        }
    }

    private void FilterPlaces()
    {
        DisplayPlaces.Clear();
        var filteredList = _allPlaces;

        if (_selectedCategory != "Phổ biến")
        {
            filteredList = _allPlaces.Where(p => p.Category == _selectedCategory).ToList();
        }

        foreach (var place in filteredList)
        {
            DisplayPlaces.Add(place);
        }
    }

    // Hàm mới: Xử lý khi người dùng bấm vào các nút Danh mục (Phổ biến, Ẩm thực...)
    private void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string category)
        {
            SelectedCategory = category;
        }
    }

    // Hàm chuyển trang khi bấm vào Thẻ địa điểm
    private async void OnPlaceSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Place selectedPlace)
        {
            await Navigation.PushAsync(new DetailPage(selectedPlace));
            ((CollectionView)sender).SelectedItem = null; 
        }
    }
}