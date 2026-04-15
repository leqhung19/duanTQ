using DoAn.FRONTEND.Models;
using DoAn.Services;
using Microsoft.Maui.Devices.Sensors;

namespace DoAn.Views
{
    public partial class MapPage : ContentPage
    {
        private CancellationTokenSource? _trackingCts;
        private List<Restaurant> _restaurants = new();
        private Restaurant? _selectedPOI;

        [Obsolete]
        public MapPage()
        {
            InitializeComponent();
            mapWebView.Navigated += OnMapLoaded;
            mapWebView.Navigating += OnWebViewNavigating;
        }

        // ============ VÒNG ĐỜI ============
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _restaurants = await RestaurantService.Instance.GetAllAsync();

            var html = await GenerateMapHtmlAsync(_restaurants);
            mapWebView.Source = new HtmlWebViewSource { Html = html };

            await RequestLocationPermissionAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _trackingCts?.Cancel();
        }

        // ============ PERMISSION ============
        private async Task RequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        // ============ MAP LOADED ============
        private void OnMapLoaded(object? sender, WebNavigatedEventArgs e)
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;

            // ✅ Tự động zoom về vị trí thực ngay khi map load xong
            StartRealtimeTracking();
            GetInitialLocationAsync();
        }

        // Lấy vị trí lần đầu và zoom vào ngay
        private async void GetInitialLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(
                    GeolocationAccuracy.Medium,
                    TimeSpan.FromSeconds(8));

                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    var ci = System.Globalization.CultureInfo.InvariantCulture;
                    var lat = location.Latitude.ToString(ci);
                    var lng = location.Longitude.ToString(ci);
                    var acc = ((int)(location.Accuracy ?? 20)).ToString();

                    // Zoom về vị trí thực ngay lần đầu mở map
                    await mapWebView.EvaluateJavaScriptAsync(
                        "updateUserLocation(" + lat + "," + lng + "," + acc + ");" +
                        "map.setView([" + lat + "," + lng + "], 16);"
                    );
                }
            }
            catch { }
        }

        // ============ REALTIME TRACKING ============
        private async void StartRealtimeTracking()
        {
            _trackingCts?.Cancel();
            _trackingCts = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                while (!_trackingCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var request = new GeolocationRequest(
                            GeolocationAccuracy.Medium,
                            TimeSpan.FromSeconds(5));

                        var location = await Geolocation.Default
                            .GetLocationAsync(request, _trackingCts.Token);

                        if (location != null)
                        {
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                await UpdateUserOnMapAsync(location);
                                await HighlightNearestPOIAsync(location);
                            });
                        }

                        await Task.Delay(3000, _trackingCts.Token);
                    }
                    catch (OperationCanceledException) { break; }
                    catch { await Task.Delay(5000); }
                }
            });
        }

        // Cập nhật vị trí người dùng lên map
        private async Task UpdateUserOnMapAsync(Location location)
        {
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var lat = location.Latitude.ToString(ci);
            var lng = location.Longitude.ToString(ci);
            var acc = ((int)(location.Accuracy ?? 20)).ToString();
            await mapWebView.EvaluateJavaScriptAsync(
                "updateUserLocation(" + lat + "," + lng + "," + acc + ");");
        }

        // Highlight POI gần nhất
        private async Task HighlightNearestPOIAsync(Location userLocation)
        {
            if (!_restaurants.Any()) return;

            var nearest = _restaurants
                .OrderBy(r => Location.CalculateDistance(
                    userLocation,
                    new Location(r.Latitude, r.Longitude),
                    DistanceUnits.Kilometers))
                .First();

            await mapWebView.EvaluateJavaScriptAsync(
                "highlightNearestPOI(" + nearest.Id + ");");
        }

        // ============ NHẤN VÀO POI ============
        [Obsolete]
        private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
        {
            if (!e.Url.StartsWith("poi://")) return;

            e.Cancel = true;

            var idStr = e.Url.Replace("poi://restaurant/", "");
            if (!int.TryParse(idStr, out var id)) return;

            var poi = _restaurants.FirstOrDefault(r => r.Id == id);
            if (poi == null) return;

            ShowPOIPanel(poi);
        }

        [Obsolete]
        private void ShowPOIPanel(Restaurant poi)
        {
            _selectedPOI = poi;

            POIName.Text = poi.Name;
            POIDescription.Text = poi.GetDescription(POIService.Instance.CurrentLang);
            POIImage.Source = poi.Image;
            POIDetailPanel.IsVisible = true;

            // Animation trượt lên
            POIDetailPanel.TranslationY = 300;
            POIDetailPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
        }

        private void OnClosePOIPanel(object? sender, EventArgs e)
        {
            POIDetailPanel.IsVisible = false;
            _selectedPOI = null;
        }

        private async void OnViewPOIDetail(object? sender, EventArgs e)
        {
            if (_selectedPOI == null) return;
            POIDetailPanel.IsVisible = false;
            await Shell.Current.GoToAsync(nameof(DetailPage),
                new Dictionary<string, object> { { "Restaurant", _selectedPOI } });
        }

        // ============ NÚT LOCATE ============
        private async void OnLocateClicked(object? sender, EventArgs e)
        {
            LocateBtn.IsEnabled = false;
            LocateBtn.Text = "⏳";
            try
            {
                var request = new GeolocationRequest(
                    GeolocationAccuracy.Medium,
                    TimeSpan.FromSeconds(8));

                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    var ci = System.Globalization.CultureInfo.InvariantCulture;
                    var lat = location.Latitude.ToString(ci);
                    var lng = location.Longitude.ToString(ci);
                    await mapWebView.EvaluateJavaScriptAsync(
                        "flyToUser(" + lat + "," + lng + ");");
                }
                else
                {
                    await DisplayAlertAsync("Thông báo", "Không lấy được vị trí", "OK");
                }
            }
            catch
            {
                await DisplayAlertAsync("Lỗi", "Không thể truy cập GPS", "OK");
            }
            finally
            {
                LocateBtn.IsEnabled = true;
                LocateBtn.Text = "📍";
            }
        }

        // ============ LOAD HTML TỪ FILE ============
        private async Task<string> GenerateMapHtmlAsync(List<Restaurant> restaurants)
        {
            // Đọc file map.html từ Resources/Raw/
            using var stream = await FileSystem.OpenAppPackageFileAsync("map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            // Tạo JS array POI
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var poisJs = "[" + string.Join(",", restaurants.Select(r =>
                "{id:" + r.Id +
                ",lat:" + r.Latitude.ToString(ci) +
                ",lng:" + r.Longitude.ToString(ci) +
                ",name:'" + (r.Name ?? "").Replace("'", "\\'") +
                "',radius:" + r.RadiusMeters.ToString(ci) + "}"
            )) + "]";

            // Thay placeholder trong HTML
            return html.Replace("POIS_PLACEHOLDER", poisJs);
        }
    }
}