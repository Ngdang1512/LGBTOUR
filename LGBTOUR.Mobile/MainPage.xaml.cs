using System.Collections.ObjectModel;
using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services;

namespace LGBTOUR.Mobile;

public partial class MainPage : ContentPage
{
    private List<Place> _allPlaces = new();
    public ObservableCollection<Place> DisplayPlaces { get; set; } = new();

    private TourApiService? _apiService;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _apiService ??= IPlatformApplication.Current?.Services.GetService<TourApiService>();

        if (_allPlaces.Count == 0)
        {
            await LoadDataAsync();
        }
        else
        {
            FilterPlaces();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _apiService ??= IPlatformApplication.Current?.Services.GetService<TourApiService>();

            if (_apiService != null)
            {
                // dùng dữ liệu đúng PRD
                var places = await _apiService.GetProjectPlacesAsync();

                _allPlaces = (places ?? new List<Place>())
                    .Where(p => p != null && p.Latitude != 0 && p.Longitude != 0)
                    .OrderByDescending(p => p.Priority)
                    .ToList();

                FilterPlaces();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể tải dữ liệu: " + ex.Message, "OK");
        }
    }

    private void FilterPlaces()
    {
        DisplayPlaces.Clear();

        foreach (var place in _allPlaces)
            DisplayPlaces.Add(place);
    }

    private async void OnPlaceSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Place selectedPlace)
        {
            await Navigation.PushAsync(new DetailPage(selectedPlace));
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}