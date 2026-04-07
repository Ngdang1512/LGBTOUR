using System.Globalization;
using System.Text;
using LGBTOUR.Mobile.Models;
using LGBTOUR.Mobile.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LGBTOUR.Mobile;

public partial class MapPage : ContentPage
{
    private readonly List<Place> _places = new();
    private TourApiService? _apiService;

    public MapPage()
    {
        InitializeComponent();
    }

    public MapPage(IEnumerable<Place> places)
        : this()
    {
        if (places != null)
        {
            _places.AddRange(places.Where(p => p != null));
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_places.Count == 0)
        {
            await LoadDataAsync();
        }

        LoadPinsAndHeat();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _apiService ??= IPlatformApplication.Current?.Services.GetService<TourApiService>();
            if (_apiService == null)
            {
                GpsStatusLabel.Text = "Không khởi tạo được dữ liệu bản đồ.";
                return;
            }

            var places = await _apiService.GetProjectPlacesAsync();
            _places.Clear();
            _places.AddRange((places ?? new List<Place>())
                .Where(p => p != null && p.Latitude != 0 && p.Longitude != 0));
        }
        catch (Exception ex)
        {
            GpsStatusLabel.Text = "Lỗi tải dữ liệu map: " + ex.Message;
        }
    }

    private void LoadPinsAndHeat()
    {
        MyMap.Pins.Clear();
        MyMap.MapElements.Clear();

        var list = _places
            .Where(p => p.Latitude != 0 && p.Longitude != 0)
            .OrderByDescending(p => p.Priority)
            .ToList();

        if (!list.Any())
        {
            // Center mặc định Quận 4 nếu chưa có dữ liệu
            var q4Center = new Location(10.7606, 106.7040);
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(q4Center, Distance.FromKilometers(2.5)));
            GpsStatusLabel.Text = "Không có dữ liệu địa điểm hợp lệ.";
            return;
        }

        foreach (var p in list)
        {
            MyMap.Pins.Add(new Pin
            {
                Label = p.Name,
                Address = p.Location,
                Location = new Location(p.Latitude, p.Longitude),
                Type = PinType.Place
            });

            var isFood = ContainsNormalized(p.Category, "Ẩm thực");
            var clampedPriority = Math.Clamp(p.Priority, 1, 10);
            var alpha = 0.18f + (clampedPriority / 10f) * 0.52f; // 0.18 -> 0.70
            var baseColor = isFood ? Color.FromArgb("#FF7043") : Color.FromArgb("#42A5F5");
            var fill = baseColor.WithAlpha(alpha);

            // Vùng heat lớn hơn nếu Priority cao
            var radius = Math.Clamp((p.TriggerRadius * 5) + (clampedPriority * 12), 120, 420);

            MyMap.MapElements.Add(new Circle
            {
                Center = new Location(p.Latitude, p.Longitude),
                Radius = Distance.FromMeters(radius),
                StrokeColor = Colors.Transparent,
                FillColor = fill
            });
        }

        var first = list.First();
        MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(first.Latitude, first.Longitude),
            Distance.FromKilometers(2.5)));

        GpsStatusLabel.Text = $"Đã tải {list.Count} địa điểm | Pin: {MyMap.Pins.Count}";
    }

    private static bool ContainsNormalized(string? source, string keyword)
        => NormalizeText(source).Contains(NormalizeText(keyword));

    private static string NormalizeText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var formD = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}