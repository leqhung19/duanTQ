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

        // ✅ Lưu 2 vị trí gần nhất để tính hướng di chuyển
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
                            // ✅ Cập nhật lịch sử vị trí để tính hướng
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
            // Debounce 5 giây
            if ((DateTime.Now - _lastTriggerTime).TotalSeconds < DebounceSeconds)
                return;

            // Lọc các POI trong bán kính
            var nearbyPOIs = new List<(Restaurant poi, double distance, double score)>();

            foreach (var poi in _pois)
            {
                var poiLocation = new Location(poi.Latitude, poi.Longitude);
                var distanceM = Location.CalculateDistance(
                    userLocation, poiLocation,
                    DistanceUnits.Kilometers) * 1000;

                if (distanceM > poi.RadiusMeters) continue;

                // Kiểm tra cooldown
                if (_cooldownMap.TryGetValue(poi.Id, out var lastTime))
                    if ((DateTime.Now - lastTime).TotalMinutes < CooldownMinutes)
                        continue;

                // ✅ Tính score dựa trên khoảng cách + hướng
                var score = CalculateScore(poi, distanceM, userLocation);
                nearbyPOIs.Add((poi, distanceM, score));
            }

            if (!nearbyPOIs.Any()) return;

            // Chọn POI có score cao nhất
            var best = nearbyPOIs.OrderByDescending(x => x.score).First();

            _lastTriggerTime = DateTime.Now;
            _cooldownMap[best.poi.Id] = DateTime.Now;

            MainThread.BeginInvokeOnMainThread(() =>
                OnPOIEntered?.Invoke(best.poi));
        }

        // ✅ Tính score = khoảng cách (50%) + hướng di chuyển (50%)
        private double CalculateScore(Restaurant poi, double distanceM, Location userLocation)
        {
            // --- Điểm khoảng cách ---
            // Càng gần → điểm càng cao (max 100)
            // VD: 5m → 100đ, 15m → 50đ, 30m → 0đ
            double maxRadius = poi.RadiusMeters;
            double distanceScore = Math.Max(0, (1 - distanceM / maxRadius)) * 100;

            // --- Điểm hướng di chuyển ---
            double directionScore = 0;

            if (_previousLocation != null && _currentLocation != null)
            {
                // Tính hướng đang đi (bearing từ vị trí cũ → mới)
                double movingBearing = CalculateBearing(
                    _previousLocation.Latitude, _previousLocation.Longitude,
                    _currentLocation.Latitude, _currentLocation.Longitude);

                // Tính hướng từ vị trí hiện tại → POI
                double poiBearing = CalculateBearing(
                    userLocation.Latitude, userLocation.Longitude,
                    poi.Latitude, poi.Longitude);

                // Tính góc lệch giữa hướng đi và hướng đến POI
                double angleDiff = Math.Abs(movingBearing - poiBearing);
                if (angleDiff > 180) angleDiff = 360 - angleDiff;

                // Góc lệch càng nhỏ → đang đi về phía POI → điểm cao
                // 0°  → 100đ (đi thẳng vào)
                // 90° → 50đ  (đi ngang)
                // 180°→ 0đ   (đi ngược chiều)
                directionScore = Math.Max(0, (1 - angleDiff / 180.0)) * 100;
            }
            else
            {
                // Chưa có lịch sử vị trí → chỉ dùng khoảng cách
                directionScore = 50; // điểm trung bình
            }

            // Tổng score: khoảng cách 50% + hướng 50%
            return (distanceScore * 0.5) + (directionScore * 0.5);
        }

        // ✅ Tính bearing (góc hướng) giữa 2 tọa độ
        // Kết quả: 0° = Bắc, 90° = Đông, 180° = Nam, 270° = Tây
        private double CalculateBearing(
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

        private double ToRad(double deg) => deg * Math.PI / 180;
        private double ToDeg(double rad) => rad * 180 / Math.PI;

        public void ResetCooldown(int poiId) => _cooldownMap.Remove(poiId);
        public void ResetAllCooldowns() => _cooldownMap.Clear();
    }
}