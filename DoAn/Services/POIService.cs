using DoAn.FRONTEND.Models;
using Microsoft.Maui.Devices.Sensors;

namespace DoAn.Services
{
    public class POIService
    {
        private static POIService? _instance;
        public static POIService Instance => _instance ??= new POIService();

        public event Action<Restaurant>? OnPOIEntered;

        private List<Restaurant> _pois = new();
        private CancellationTokenSource? _trackingCts;
        private bool _isTracking = false;

        private DateTime _lastTriggerTime = DateTime.MinValue;
        private const int DebounceSeconds = 5;

        private readonly Dictionary<int, DateTime> _cooldownMap = new();
        private const int CooldownMinutes = 3;

        private Location? _previousLocation;
        private Location? _currentLocation;

        public string CurrentLang { get; set; } = "vi";

        public void SetPOIs(List<Restaurant> restaurants)
        {
            _pois = restaurants
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }

        public async Task StartTrackingAsync()
        {
            if (_isTracking) return;
            _isTracking = true;
            _trackingCts = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                while (!_trackingCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var request = new GeolocationRequest(
                            GeolocationAccuracy.Medium,
                            TimeSpan.FromSeconds(8));

                        var location = await Geolocation.Default
                            .GetLocationAsync(request, _trackingCts.Token);

                        if (location != null)
                        {
                            _previousLocation = _currentLocation;
                            _currentLocation = location;
                            CheckProximity(location);
                        }

                        await Task.Delay(3000, _trackingCts.Token);
                    }
                    catch (OperationCanceledException) { break; }
                    catch { await Task.Delay(5000); }
                }

                _isTracking = false;
            });
        }

        public void StopTracking()
        {
            _isTracking = false;
            _trackingCts?.Cancel();
        }

        private void CheckProximity(Location userLocation)
        {
            if ((DateTime.Now - _lastTriggerTime).TotalSeconds < DebounceSeconds)
                return;

            var nearbyPOIs = new List<(Restaurant poi, double distance, double score)>();

            foreach (var poi in _pois)
            {
                var poiLocation = new Location(poi.Latitude, poi.Longitude);
                var distanceM = Location.CalculateDistance(
                    userLocation, poiLocation,
                    DistanceUnits.Kilometers) * 1000;

                if (distanceM > poi.RadiusMeters) continue;

                if (_cooldownMap.TryGetValue(poi.Id, out var lastTime)
                    && (DateTime.Now - lastTime).TotalMinutes < CooldownMinutes)
                {
                    continue;
                }

                var score = CalculateScore(poi, distanceM, userLocation);
                nearbyPOIs.Add((poi, distanceM, score));
            }

            if (!nearbyPOIs.Any()) return;

            var best = nearbyPOIs
                .OrderByDescending(x => x.score)
                .ThenBy(x => x.distance)
                .First();

            _lastTriggerTime = DateTime.Now;
            _cooldownMap[best.poi.Id] = DateTime.Now;

            MainThread.BeginInvokeOnMainThread(() =>
                OnPOIEntered?.Invoke(best.poi));
        }

        private double CalculateScore(Restaurant poi, double distanceM, Location userLocation)
        {
            var maxRadius = Math.Max(poi.RadiusMeters, 1);
            var distanceScore = Math.Max(0, (1 - distanceM / maxRadius)) * 100;

            double directionScore;
            if (_previousLocation != null && _currentLocation != null)
            {
                var movingBearing = CalculateBearing(
                    _previousLocation.Latitude, _previousLocation.Longitude,
                    _currentLocation.Latitude, _currentLocation.Longitude);

                var poiBearing = CalculateBearing(
                    userLocation.Latitude, userLocation.Longitude,
                    poi.Latitude, poi.Longitude);

                var angleDiff = Math.Abs(movingBearing - poiBearing);
                if (angleDiff > 180) angleDiff = 360 - angleDiff;

                directionScore = Math.Max(0, (1 - angleDiff / 180.0)) * 100;
            }
            else
            {
                directionScore = 50;
            }

            return (distanceScore * 0.5) + (directionScore * 0.5);
        }

        private static double CalculateBearing(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            var dLon = ToRad(lon2 - lon1);
            var dLat1 = ToRad(lat1);
            var dLat2 = ToRad(lat2);

            var y = Math.Sin(dLon) * Math.Cos(dLat2);
            var x = Math.Cos(dLat1) * Math.Sin(dLat2)
                  - Math.Sin(dLat1) * Math.Cos(dLat2) * Math.Cos(dLon);

            var bearing = Math.Atan2(y, x);
            return (ToDeg(bearing) + 360) % 360;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
        private static double ToDeg(double rad) => rad * 180 / Math.PI;

        public void ResetCooldown(int poiId) => _cooldownMap.Remove(poiId);
        public void ResetAllCooldowns() => _cooldownMap.Clear();
    }
}
