using System.Collections.ObjectModel;
using System.Net.Http;
using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;

namespace SaigonAudioTour.Mobile;

public partial class MainPage : ContentPage
{
    private const string NarratingPlaceKey = "NarratingPlaceId";

    private static readonly HttpClient _imageWarmupClient = new()
    {
        Timeout = TimeSpan.FromSeconds(6)
    };

    private List<Place> _allPlaces = new();
    public ObservableCollection<Place> DisplayPlaces { get; set; } = new();

    private TourApiService? _apiService;
    private bool _isLoading;
    private bool _isLoadingPlaces;
    private bool _isNavigatingToDetail;

    public bool IsLoadingPlaces
    {
        get => _isLoadingPlaces;
        set
        {
            if (_isLoadingPlaces == value) return;
            _isLoadingPlaces = value;
            OnPropertyChanged();
        }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoading) return;

        _apiService ??= IPlatformApplication.Current?.Services.GetService<TourApiService>();

        if (_allPlaces.Count == 0)
        {
            await LoadDataAsync();
        }
        else
        {
            SyncNarratingState();
            FilterPlaces();
        }
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        IsLoadingPlaces = true;
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

                SyncNarratingState();

                DisplayPlaces = new ObservableCollection<Place>(_allPlaces);
                OnPropertyChanged(nameof(DisplayPlaces));

                _ = Task.Run(() => WarmupTopImagesAsync(_allPlaces));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", "Không thể tải dữ liệu: " + ex.Message, "OK");
        }
        finally
        {
            IsLoadingPlaces = false;
            _isLoading = false;
        }
    }

    private void FilterPlaces()
    {
        SyncNarratingState();
        DisplayPlaces = new ObservableCollection<Place>(_allPlaces);
        OnPropertyChanged(nameof(DisplayPlaces));
    }

    private void SyncNarratingState()
    {
        var narratingId = Preferences.Get(NarratingPlaceKey, -1);
        foreach (var place in _allPlaces)
        {
            place.IsNarrating = place.Id == narratingId;
        }
    }

    private async void OnPlaceSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Place selectedPlace)
        {
            await Navigation.PushAsync(new DetailPage(selectedPlace));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (_isNavigatingToDetail) return;
        if (sender is not Border card) return;
        if (card.BindingContext is not Place selectedPlace) return;

        _isNavigatingToDetail = true;
        try
        {
            await card.ScaleToAsync(0.975, 70, Easing.CubicOut);
            await card.ScaleToAsync(1, 90, Easing.CubicIn);
            await Navigation.PushAsync(new DetailPage(selectedPlace));
        }
        finally
        {
            _isNavigatingToDetail = false;
        }
    }

    private static async Task WarmupTopImagesAsync(IEnumerable<Place> places)
    {
        var urls = places
            .Select(p => p.ImageUrl)
            .Where(u => !string.IsNullOrWhiteSpace(u) && Uri.TryCreate(u, UriKind.Absolute, out _))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList();

        foreach (var url in urls)                                                                                                                                                                                                       
        {
            try
            {
                using var response = await _imageWarmupClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode) continue;

                await using var stream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[2048];
                _ = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
            }
            catch
            {
                // Bỏ qua lỗi warmup để không ảnh hưởng UI
            }
        }
    }
}