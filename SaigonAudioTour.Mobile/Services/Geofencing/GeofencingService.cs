using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using SaigonAudioTour.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SaigonAudioTour.Mobile.Services.Geofencing
{
    /// <summary>
    /// Event args khi phát hiện nearby POI
    /// </summary>
    public class NearbyPoiEventArgs : EventArgs
    {
        public Place? Poi { get; set; }
        public double DistanceMeters { get; set; }
        public Location? CurrentLocation { get; set; }
    }

    /// <summary>
    /// Background geofencing service - track GPS mỗi 30s, trigger narration khi vào geofence
    /// </summary>
    public class GeofencingService
    {
        private readonly PoiApiService _poiApiService;
        private readonly GeofenceSessionState _sessionState;
        private readonly GeofenceConflictResolver _conflictResolver;

        private CancellationTokenSource? _cancellationTokenSource;
        private Location? _lastKnownLocation;
        private bool _isMonitoring;

        /// <summary>
        /// Event trigger khi detect nearby POI
        /// </summary>
        public event EventHandler<NearbyPoiEventArgs>? OnNearbyPoiDetected;

        public GeofencingService(PoiApiService poiApiService, GeofenceSessionState sessionState)
        {
            _poiApiService = poiApiService;
            _sessionState = sessionState;
            _conflictResolver = new GeofenceConflictResolver();
            _conflictResolver.OnResolved += OnPoiResolved;
            _isMonitoring = false;
        }

        /// <summary>
        /// Bắt đầu background GPS monitoring
        /// Mỗi 30s capture location, so sánh với POI geofence radius
        /// </summary>
        public async Task StartMonitoringAsync()
        {
            if (_isMonitoring)
                return;

            _isMonitoring = true;
            _cancellationTokenSource = new CancellationTokenSource();

            // Check location permissions (foreground + background)
            var whenInUseStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (whenInUseStatus != PermissionStatus.Granted)
            {
                whenInUseStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (whenInUseStatus != PermissionStatus.Granted)
                {
                    _isMonitoring = false;
                    return;
                }
            }

            var alwaysStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (alwaysStatus != PermissionStatus.Granted)
            {
                alwaysStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();
                if (alwaysStatus != PermissionStatus.Granted)
                {
                    // App vẫn có thể chạy khi foreground, nhưng background geofence sẽ bị giới hạn
                    System.Diagnostics.Debug.WriteLine("[GeofencingService] LocationAlways permission not granted.");
                }
            }

            // Background GPS monitoring task
            _ = Task.Run(async () => await MonitorLocationAsync(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Dừng background GPS monitoring
        /// </summary>
        public void StopMonitoring()
        {
            _isMonitoring = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _lastKnownLocation = null;
            _sessionState.ClearActivePoi();
        }

        private void OnPoiResolved(object? sender, QueuedPoi decision)
        {
            var location = _lastKnownLocation;
            OnNearbyPoiDetected?.Invoke(this, new NearbyPoiEventArgs
            {
                Poi = decision.Poi,
                DistanceMeters = decision.DistanceMeters,
                CurrentLocation = location
            });

            if (location != null)
            {
                _sessionState.SetActivePoi(decision.Poi, decision.DistanceMeters, location);
                _sessionState.SetActivityStatus("listening");
            }

            GeofenceHelper.MarkPoiAsPlayed(decision.Poi.Id);
        }

        private async Task MonitorLocationAsync(CancellationToken cancellationToken)
        {
            // Load POI list
            var places = await _poiApiService.GetPlacesAsync();
            if (places == null || !places.Any())
                return;

            _conflictResolver.Start();

            // Monitoring loop - track location mỗi 30 giây
            while (_isMonitoring && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Get current location
                    var location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(
                            GeolocationAccuracy.Default,
                            TimeSpan.FromSeconds(5)
                        )
                    );

                    if (location != null)
                    {
                        // Check debounce - ignore nếu di chuyển < 10m
                        if (GeofenceHelper.ShouldIgnoreLocationUpdate(_lastKnownLocation, location))
                        {
                            await Task.Delay(30000, cancellationToken); // 30 giây
                            continue;
                        }

                        _lastKnownLocation = location;
                        _sessionState.SetActivityStatus("moving");

                        var notPlayedPlaces = places
                            .Where(poi => !GeofenceHelper.IsPoiAlreadyPlayed(poi.Id))
                            .ToList();

                        await _conflictResolver.EvaluateAndQueueAsync(
                            new LocationPoint(location.Latitude, location.Longitude),
                            notPlayedPlaces,
                            cancellationToken);

                        // Fallback: nếu tất cả đã phát hết thì cho phép chạy lại theo session mới
                        if (notPlayedPlaces.Count == 0)
                        {
                            GeofenceHelper.ClearPlayedPois();
                        }
                    }

                    // Wait 30 seconds before next location check
                    await Task.Delay(30000, cancellationToken);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[GeofencingService] Error: {ex.Message}");
                    await Task.Delay(5000, cancellationToken); // Retry sau 5 giây
                }
            }
        }

        public bool IsMonitoring => _isMonitoring;
    }
}
