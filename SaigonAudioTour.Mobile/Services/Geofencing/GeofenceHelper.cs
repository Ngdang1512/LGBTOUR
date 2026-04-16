using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;

namespace SaigonAudioTour.Mobile.Services.Geofencing
{
    /// <summary>
    /// Helper class cho geofence calculations và anti-spam logic
    /// </summary>
    public static class GeofenceHelper
    {
        /// <summary>
        /// Tính khoảng cách giữa 2 tọa độ dùng Haversine formula
        /// Trả về khoảng cách theo mét
        /// </summary>
        public static double CalculateHaversineDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double earthRadiusKm = 6371.0; // Bán kính Trái Đất (km)

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLng = DegreesToRadians(lng2 - lng1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = earthRadiusKm * c; // Khoảng cách theo km

            return distance * 1000; // Chuyển sang mét
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        /// <summary>
        /// Check xem POI này đã được phát trong phiên hiện tại không
        /// Dùng Preferences để lưu danh sách POI đã phát
        /// </summary>
        public static bool IsPoiAlreadyPlayed(int poiId)
        {
            var playedPoisJson = Preferences.Get("PlayedPois", "[]");
            var playedPois = System.Text.Json.JsonSerializer.Deserialize<List<int>>(playedPoisJson) ?? new();
            return playedPois.Contains(poiId);
        }

        /// <summary>
        /// Mark POI đã phát
        /// </summary>
        public static void MarkPoiAsPlayed(int poiId)
        {
            var playedPoisJson = Preferences.Get("PlayedPois", "[]");
            var playedPois = System.Text.Json.JsonSerializer.Deserialize<List<int>>(playedPoisJson) ?? new();

            if (!playedPois.Contains(poiId))
            {
                playedPois.Add(poiId);
                var updated = System.Text.Json.JsonSerializer.Serialize(playedPois);
                Preferences.Set("PlayedPois", updated);
            }
        }

        /// <summary>
        /// Clear danh sách POI đã phát (khi kết thúc tour)
        /// </summary>
        public static void ClearPlayedPois()
        {
            Preferences.Remove("PlayedPois");
        }

        /// <summary>
        /// Check xem có nên debounce/ignore location update này không
        /// Dùng để tránh phát quá nhiều lần trong thời gian ngắn
        /// </summary>
        public static bool ShouldIgnoreLocationUpdate(Location? lastKnownLocation, Location currentLocation, double minDistanceMeters = 10.0)
        {
            if (lastKnownLocation == null)
                return false;

            var distance = CalculateHaversineDistance(
                lastKnownLocation.Latitude,
                lastKnownLocation.Longitude,
                currentLocation.Latitude,
                currentLocation.Longitude
            );

            // Ignore nếu di chuyển < 10 mét
            return distance < minDistanceMeters;
        }
    }
}
