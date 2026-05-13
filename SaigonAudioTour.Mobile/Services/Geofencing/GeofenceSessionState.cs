using SaigonAudioTour.Mobile.Models;
using Microsoft.Maui.Devices.Sensors;
using System;

namespace SaigonAudioTour.Mobile.Services.Geofencing
{
    /// <summary>
    /// Shared runtime state for the currently active geofence / narration.
    /// </summary>
    public class GeofenceSessionState
    {
        private Place? _activePoi;
        private double _distanceMeters;
        private Location? _currentLocation;
        private string _activityStatus = "idle";
        private DateTimeOffset _lastUpdatedAt = DateTimeOffset.MinValue;

        public event EventHandler? Changed;

        public Place? ActivePoi => _activePoi;
        public int ActivePoiId => _activePoi?.Id ?? -1;
        public double DistanceMeters => _distanceMeters;
        public Location? CurrentLocation => _currentLocation;
        public string ActivityStatus => _activityStatus;
        public DateTimeOffset LastUpdatedAt => _lastUpdatedAt;
        public bool HasActivePoi => _activePoi != null;

        public void SetActivePoi(Place? poi, double distanceMeters, Location? currentLocation)
        {
            _activePoi = poi;
            _distanceMeters = distanceMeters;
            _currentLocation = currentLocation;
            _lastUpdatedAt = DateTimeOffset.UtcNow;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void SetActivityStatus(string? status)
        {
            var normalizedStatus = string.IsNullOrWhiteSpace(status)
                ? "idle"
                : status.Trim().ToLowerInvariant();

            if (_activityStatus == normalizedStatus)
            {
                return;
            }

            _activityStatus = normalizedStatus;
            _lastUpdatedAt = DateTimeOffset.UtcNow;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void ClearActivePoi()
        {
            if (_activePoi == null && _currentLocation == null && _distanceMeters <= 0)
            {
                return;
            }

            _activePoi = null;
            _distanceMeters = 0;
            _currentLocation = null;
            _activityStatus = "idle";
            _lastUpdatedAt = DateTimeOffset.UtcNow;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
