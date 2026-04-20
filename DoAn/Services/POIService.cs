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

        // Debounce 5 giây
        private DateTime _lastTriggerTime = DateTime.MinValue;
        private const int DebounceSeconds = 5;

        // Cooldown 3 phút mỗi POI
        private readonly Dictionary<int, DateTime> _cooldownMap = new();
        private const int CooldownMinutes = 3;

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
                            TimeSpan.FromSeconds(8)
                        );
                        var location = await Geolocation.Default
                            .GetLocationAsync(request, _trackingCts.Token);

                        if (location != null)
                            CheckProximity(location);

                        // Check mỗi 3 giây — tiết kiệm pin
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
            // Debounce 5 giây
            if ((DateTime.Now - _lastTriggerTime).TotalSeconds < DebounceSeconds)
                return;

            Restaurant? nearestPOI = null;
            double nearestDistance = double.MaxValue;

            foreach (var poi in _pois)
            {
                var poiLocation = new Location(poi.Latitude, poi.Longitude);
                var distanceM = Location.CalculateDistance(
                    userLocation, poiLocation,
                    DistanceUnits.Kilometers) * 1000;

                if (distanceM > poi.RadiusMeters) continue;
                if (distanceM >= nearestDistance) continue;

                // Kiểm tra cooldown 3 phút
                if (_cooldownMap.TryGetValue(poi.Id, out var lastTime))
                    if ((DateTime.Now - lastTime).TotalMinutes < CooldownMinutes)
                        continue;

                nearestDistance = distanceM;
                nearestPOI = poi;
            }

            if (nearestPOI == null) return;

            _lastTriggerTime = DateTime.Now;
            _cooldownMap[nearestPOI.Id] = DateTime.Now;

            MainThread.BeginInvokeOnMainThread(() =>
                OnPOIEntered?.Invoke(nearestPOI));
        }

        public void ResetCooldown(int poiId) => _cooldownMap.Remove(poiId);
        public void ResetAllCooldowns() => _cooldownMap.Clear();
    }
}