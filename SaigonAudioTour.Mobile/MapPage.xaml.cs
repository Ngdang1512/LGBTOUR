using SaigonAudioTour.Mobile.Models;
using SaigonAudioTour.Mobile.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SaigonAudioTour.Mobile;

public partial class MapPage : ContentPage
{
    private static readonly HttpClient _routeHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private readonly List<Place> _places = new();
    private TourApiService? _apiService;
    private List<Place> _lastOrderedStops = new();
    private bool _hasRenderedMap;
    private bool _isMapKeyMissing;
    private bool _isFallbackMode;

#if ANDROID
    private const bool PreferCompatMap = true;
#else
    private const bool PreferCompatMap = false;
#endif

    public bool IsMapKeyMissing
    {
        get => _isMapKeyMissing;
        set
        {
            if (_isMapKeyMissing == value) return;
            _isMapKeyMissing = value;
            OnPropertyChanged();
        }
    }

    public bool IsFallbackMode
    {
        get => _isFallbackMode;
        set
        {
            if (_isFallbackMode == value) return;
            _isFallbackMode = value;
            OnPropertyChanged();
        }
    }

    public MapPage()
    {
        InitializeComponent();
        BindingContext = this;
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

        ValidateMapProvider();

        if (_places.Count == 0)
        {
            await LoadDataAsync();
        }

        IsFallbackMode = PreferCompatMap || IsMapKeyMissing;

        if (!IsFallbackMode && !_hasRenderedMap)
        {
            await LoadPinsAndHeatAsync();
            _hasRenderedMap = true;
        }

        if (IsFallbackMode)
        {
            GpsStatusLabel.Text = "Đang dùng OpenStreetMap (compat mode).";
            await LoadOsmFallbackAsync();
        }
    }

    private void ValidateMapProvider()
    {
#if ANDROID
        try
        {
            var context = global::Android.App.Application.Context;
            var keyResId = context?.Resources?.GetIdentifier("google_maps_api_key", "string", context.PackageName) ?? 0;
            var apiKey = keyResId > 0 ? context?.GetString(keyResId) : null;
            IsMapKeyMissing = string.IsNullOrWhiteSpace(apiKey)
                              || apiKey.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            IsMapKeyMissing = true;
        }
#else
        IsMapKeyMissing = false;
#endif
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

    private async Task LoadPinsAndHeatAsync()
    {
        MyMap.Pins.Clear();
        MyMap.MapElements.Clear();

        var list = _places
            .Where(p => p.Latitude != 0 && p.Longitude != 0)
            .OrderBy(p => p.Id)
            .ToList();

        if (!list.Any())
        {
            // Center mặc định Quận 1 nếu chưa có dữ liệu
            var q1Center = new Location(10.7755, 106.7008);
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(q1Center, Distance.FromKilometers(2.5)));
            GpsStatusLabel.Text = "Không có dữ liệu địa điểm hợp lệ.";
            return;
        }

        // Đi đúng thứ tự dữ liệu đã khai báo và khép vòng về điểm đầu (Chợ Bến Thành)
        var orderedStops = BuildFixedLoopStops(list);
        _lastOrderedStops = orderedStops;

        // Tuyến theo đường thật, màu đơn giản để đỡ rối mắt
        var roadPath = await GetShortestRoadPathAsync(orderedStops);
        if (roadPath.Count >= 2)
        {
            var routeLine = new Polyline
            {
                StrokeColor = Color.FromArgb("#2563EB"),
                StrokeWidth = 6
            };

            foreach (var loc in roadPath)
            {
                routeLine.Geopath.Add(loc);
            }

            MyMap.MapElements.Add(routeLine);
        }

        for (var idx = 0; idx < orderedStops.Count; idx++)
        {
            var p = orderedStops[idx];
            var isStart = idx == 0;
            var isEnd = idx == orderedStops.Count - 1;
            var poiEmoji = GetPoiEmoji(p);

            // Điểm cuối trùng điểm đầu (vòng khép kín), không thêm pin trùng
            if (isEnd && p.Id == orderedStops[0].Id)
            {
                continue;
            }

            MyMap.Pins.Add(new Pin
            {
                Label = isStart
                    ? $"🟢🏁 {p.Name}"
                    : isEnd
                        ? $"🔴 {p.Name}"
                        : $"{idx + 1}. {poiEmoji} {p.Name}",
                Address = p.Location,
                Location = new Location(p.Latitude, p.Longitude),
                Type = PinType.Place
            });
        }

        // Đánh dấu rõ điểm bắt đầu/kết thúc (cùng 1 vị trí Chợ Bến Thành)
        var start = orderedStops[0];
        var startCenter = new Location(start.Latitude, start.Longitude);
        MyMap.MapElements.Add(new Circle
        {
            Center = startCenter,
            Radius = Distance.FromMeters(70),
            StrokeColor = Color.FromArgb("#16A34A"),
            FillColor = Color.FromArgb("#16A34A").WithAlpha(0.16f)
        });
        MyMap.MapElements.Add(new Circle
        {
            Center = startCenter,
            Radius = Distance.FromMeters(45),
            StrokeColor = Color.FromArgb("#DC2626"),
            FillColor = Color.FromArgb("#DC2626").WithAlpha(0.14f)
        });

        var first = orderedStops.First();
        MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(first.Latitude, first.Longitude),
            Distance.FromKilometers(2.5)));

        var startName = orderedStops.First().Name;
        GpsStatusLabel.Text = $"{orderedStops.Count - 1} điểm | Tuyến vòng: {startName} → ... → {startName}";
    }

    private async void OnOpenExternalMapClicked(object sender, EventArgs e)
    {
        var first = (_lastOrderedStops.Count > 0 ? _lastOrderedStops : _places)
            .Where(p => p.Latitude != 0 && p.Longitude != 0)
            .OrderBy(p => p.Id)
            .FirstOrDefault();

        if (first == null) return;

        var location = new Location(first.Latitude, first.Longitude);
        await global::Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, new MapLaunchOptions { Name = first.Name });
    }

    private async Task LoadOsmFallbackAsync()
    {
                var points = _places
            .Where(p => p.Latitude != 0 && p.Longitude != 0)
            .OrderBy(p => p.Id)
                        .Take(20)
                        .ToList();

                var orderedStops = BuildFixedLoopStops(points);
                _lastOrderedStops = orderedStops;

                var center = orderedStops.FirstOrDefault();
                var centerLat = (center?.Latitude ?? 10.7755).ToString(CultureInfo.InvariantCulture);
                var centerLon = (center?.Longitude ?? 106.7008).ToString(CultureInfo.InvariantCulture);

                var markersBuilder = new StringBuilder();

                for (var idx = 0; idx < orderedStops.Count; idx++)
                {
                        var p = orderedStops[idx];
                        var isStart = idx == 0;
                        var isEnd = idx == orderedStops.Count - 1;

                        if (isEnd && p.Id == orderedStops[0].Id)
                        {
                            continue;
                        }

                        var lat = p.Latitude.ToString(CultureInfo.InvariantCulture);
                        var lon = p.Longitude.ToString(CultureInfo.InvariantCulture);
                        var popup = JsonSerializer.Serialize($"{p.Name}<br/>{p.Location}");
                        var iconHtmlJson = JsonSerializer.Serialize(GetOsmIconHtml(p, isStart));

                        markersBuilder.AppendLine($"var icon{idx}=L.divIcon({{className:'poi-wrap',html:{iconHtmlJson},iconSize:[34,34],iconAnchor:[17,17],popupAnchor:[0,-12]}});");
                        markersBuilder.AppendLine($"L.marker([{lat}, {lon}], {{icon: icon{idx}}}).addTo(map).bindPopup({popup});");
                }

                var roadPath = await GetShortestRoadPathAsync(orderedStops);
                var polylinePoints = roadPath.Count >= 2
                    ? string.Join(",", roadPath.Select(l => $"[{l.Latitude.ToString(CultureInfo.InvariantCulture)},{l.Longitude.ToString(CultureInfo.InvariantCulture)}]"))
                    : string.Join(",", orderedStops.Select(p => $"[{p.Latitude.ToString(CultureInfo.InvariantCulture)},{p.Longitude.ToString(CultureInfo.InvariantCulture)}]"));

                var polylineScript = string.IsNullOrWhiteSpace(polylinePoints)
                    ? string.Empty
                    : $"L.polyline([{polylinePoints}], {{ color: '#2563EB', weight: 5, opacity: 0.92 }}).addTo(map);";

                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>
        html, body, #map {{ height: 100%; margin: 0; padding: 0; }}
        .poi-wrap {{ background: transparent; border: none; }}
        .poi-chip {{
            width: 34px;
            height: 34px;
            border-radius: 17px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 18px;
            box-shadow: 0 1px 5px rgba(15,23,42,.25);
            background: #ffffff;
            border: 2px solid #CBD5E1;
        }}
        .poi-chip.start {{
            border-color: #16A34A;
            background: #ECFDF5;
        }}
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        var map = L.map('map').setView([{centerLat}, {centerLon}], 15);
        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }}).addTo(map);
        {polylineScript}
        {markersBuilder}
    </script>
</body>
</html>";

                OsmWebView.Source = new HtmlWebViewSource { Html = html };
    }

    private static List<Place> BuildFixedLoopStops(List<Place> orderedById)
    {
        var ordered = orderedById
            .Where(p => p != null)
            .OrderBy(p => p.Id)
            .ToList();

        if (ordered.Count <= 1)
            return ordered;

        var start = ordered[0]; // Chợ Bến Thành theo dữ liệu hiện tại (Id=1)
        ordered.Add(start);
        return ordered;
    }

    private static string GetPoiEmoji(Place place)
    {
        var name = place.Name ?? string.Empty;

        if (name.Contains("Chợ", StringComparison.OrdinalIgnoreCase)) return "🛍️";
        if (name.Contains("Nhà hát", StringComparison.OrdinalIgnoreCase)) return "🎭";
        if (name.Contains("Ủy ban", StringComparison.OrdinalIgnoreCase) || name.Contains("UBND", StringComparison.OrdinalIgnoreCase)) return "🏛️";
        if (name.Contains("Dinh", StringComparison.OrdinalIgnoreCase)) return "🏰";
        if (name.Contains("Nhà thờ", StringComparison.OrdinalIgnoreCase)) return "⛪";
        if (name.Contains("Bưu điện", StringComparison.OrdinalIgnoreCase)) return "📮";
        if (name.Contains("Bảo tàng", StringComparison.OrdinalIgnoreCase)) return "🏺";
        if (name.Contains("Thảo Cầm Viên", StringComparison.OrdinalIgnoreCase)) return "🦁";
        if (name.Contains("Skydeck", StringComparison.OrdinalIgnoreCase)) return "🌆";

        return "📍";
    }

    private static string GetOsmIconHtml(Place place, bool isStart)
    {
        var emoji = isStart ? "🏁" : GetPoiEmoji(place);
        var cssClass = isStart ? "poi-chip start" : "poi-chip";
        return $"<div class='{cssClass}'>{emoji}</div>";
    }

    private void AddColoredRouteSegmentsToNativeMapInstance(List<Location> path)
    {
        var segmentColors = new[]
        {
            Color.FromArgb("#16A34A"),
            Color.FromArgb("#22C55E"),
            Color.FromArgb("#3B82F6"),
            Color.FromArgb("#F59E0B"),
            Color.FromArgb("#DC2626")
        };

        var chunks = SplitPathIntoChunks(path, segmentColors.Length);
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            if (chunk.Count < 2) continue;

            var line = new Polyline
            {
                StrokeColor = segmentColors[Math.Min(i, segmentColors.Length - 1)],
                StrokeWidth = 6
            };

            foreach (var loc in chunk)
            {
                line.Geopath.Add(loc);
            }

            MyMap.MapElements.Add(line);
        }
    }

    private static List<List<Location>> SplitPathIntoChunks(List<Location> path, int maxChunks)
    {
        var result = new List<List<Location>>();
        if (path.Count < 2) return result;

        var chunkCount = Math.Min(maxChunks, path.Count - 1);
        for (var c = 0; c < chunkCount; c++)
        {
            var start = (c * (path.Count - 1)) / chunkCount;
            var end = ((c + 1) * (path.Count - 1)) / chunkCount;

            var chunk = new List<Location>();
            for (var i = start; i <= end; i++)
            {
                chunk.Add(path[i]);
            }

            if (chunk.Count >= 2)
            {
                result.Add(chunk);
            }
        }

        return result;
    }

    private static string BuildSegmentedOsmPolylineScript(List<Location> path)
    {
        if (path.Count < 2) return string.Empty;

        var colors = new[] { "#16A34A", "#22C55E", "#3B82F6", "#F59E0B", "#DC2626" };
        var chunks = SplitPathIntoChunks(path, colors.Length);
        var sb = new StringBuilder();

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            if (chunk.Count < 2) continue;

            var chunkPoints = string.Join(",", chunk.Select(l =>
                $"[{l.Latitude.ToString(CultureInfo.InvariantCulture)},{l.Longitude.ToString(CultureInfo.InvariantCulture)}]"));

            sb.AppendLine($"L.polyline([{chunkPoints}], {{ color: '{colors[Math.Min(i, colors.Length - 1)]}', weight: 5, opacity: 0.9 }}).addTo(map);");
        }

        return sb.ToString();
    }

    private async Task<RoutePlan> GetOptimizedTripPlanAsync(List<Place> input)
    {
        if (input.Count < 2)
        {
            return new RoutePlan
            {
                OrderedStops = input,
                RoadPath = input.Select(p => new Location(p.Latitude, p.Longitude)).ToList(),
                IsOptimized = false
            };
        }

        try
        {
            var coords = string.Join(";", input.Select(p =>
                $"{p.Longitude.ToString(CultureInfo.InvariantCulture)},{p.Latitude.ToString(CultureInfo.InvariantCulture)}"));

            var tripUrl = $"https://router.project-osrm.org/trip/v1/driving/{coords}?source=first&destination=last&roundtrip=false&steps=false&overview=full&geometries=geojson";
            using var tripResponse = await _routeHttpClient.GetAsync(tripUrl);

            if (tripResponse.IsSuccessStatusCode)
            {
                await using var stream = await tripResponse.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (doc.RootElement.TryGetProperty("waypoints", out var waypoints)
                    && doc.RootElement.TryGetProperty("trips", out var trips)
                    && trips.GetArrayLength() > 0
                    && waypoints.GetArrayLength() == input.Count)
                {
                    var indexed = new List<(Place place, int order)>();
                    for (var i = 0; i < input.Count; i++)
                    {
                        var order = waypoints[i].GetProperty("waypoint_index").GetInt32();
                        indexed.Add((input[i], order));
                    }

                    var orderedStops = indexed.OrderBy(x => x.order).Select(x => x.place).ToList();

                    var geometry = trips[0].GetProperty("geometry");
                    var path = ParseGeoJsonCoordinates(geometry);

                    if (path.Count >= 2)
                    {
                        return new RoutePlan
                        {
                            OrderedStops = orderedStops,
                            RoadPath = path,
                            IsOptimized = true
                        };
                    }
                }
            }
        }
        catch
        {
            // fallback bên dưới
        }

        var fallbackPath = await GetShortestRoadPathAsync(input);
        return new RoutePlan
        {
            OrderedStops = input,
            RoadPath = fallbackPath.Count >= 2 ? fallbackPath : input.Select(p => new Location(p.Latitude, p.Longitude)).ToList(),
            IsOptimized = false
        };
    }

    private static List<Location> ParseGeoJsonCoordinates(JsonElement geometry)
    {
        var result = new List<Location>();
        if (!geometry.TryGetProperty("coordinates", out var coordinates)) return result;

        foreach (var pair in coordinates.EnumerateArray())
        {
            if (pair.GetArrayLength() < 2) continue;
            var lon = pair[0].GetDouble();
            var lat = pair[1].GetDouble();
            result.Add(new Location(lat, lon));
        }

        return result;
    }

    private sealed class RoutePlan
    {
        public required List<Place> OrderedStops { get; init; }
        public required List<Location> RoadPath { get; init; }
        public bool IsOptimized { get; init; }
    }

    private static async Task<List<Location>> GetShortestRoadPathAsync(List<Place> orderedPoints)
    {
        if (orderedPoints.Count < 2)
            return new List<Location>();

        try
        {
            // OSRM expects lon,lat
            var coords = string.Join(";",
                orderedPoints.Select(p =>
                    $"{p.Longitude.ToString(CultureInfo.InvariantCulture)},{p.Latitude.ToString(CultureInfo.InvariantCulture)}"));

            var url = $"https://router.project-osrm.org/route/v1/driving/{coords}?overview=full&geometries=geojson&steps=false";
            using var response = await _routeHttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<Location>();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                return new List<Location>();

            var geometry = routes[0].GetProperty("geometry");
            if (!geometry.TryGetProperty("coordinates", out var coordinates))
                return new List<Location>();

            var result = new List<Location>(coordinates.GetArrayLength());
            foreach (var pair in coordinates.EnumerateArray())
            {
                if (pair.GetArrayLength() < 2) continue;
                var lon = pair[0].GetDouble();
                var lat = pair[1].GetDouble();
                result.Add(new Location(lat, lon));
            }

            return result;
        }
        catch
        {
            return new List<Location>();
        }
    }

    private static bool ContainsText(string? source, string keyword)
        => !string.IsNullOrWhiteSpace(source)
           && source.Contains(keyword, StringComparison.OrdinalIgnoreCase);
}