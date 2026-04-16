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

        private async Task MonitorLocationAsync(CancellationToken cancellationToken)
        {
            // Load POI list
            var places = await _poiApiService.GetPlacesAsync();
            if (places == null || !places.Any())
                return;

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

                        // Check mỗi POI xem có trong geofence không
                        foreach (var poi in places)
                        {
                            var distance = GeofenceHelper.CalculateHaversineDistance(
                                location.Latitude,
                                location.Longitude,
                                poi.Latitude,
                                poi.Longitude
                            );

                            // Detect nearby: nếu trong radius của POI
                            if (distance <= poi.TriggerRadius)
                            {
                                // Anti-spam: check xem đã phát POI này trong phiên chưa
                                if (!GeofenceHelper.IsPoiAlreadyPlayed(poi.Id))
                                {
                                    // Trigger event
                                    OnNearbyPoiDetected?.Invoke(this, new NearbyPoiEventArgs
                                    {
                                        Poi = poi,
                                        DistanceMeters = distance,
                                        CurrentLocation = location
                                    });

                                    _sessionState.SetActivePoi(poi, distance, location);

                                    // Mark as played
                                    GeofenceHelper.MarkPoiAsPlayed(poi.Id);
                                }
                            }
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
