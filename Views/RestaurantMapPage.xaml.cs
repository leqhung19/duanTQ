using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace DoAn.Views
{
    [QueryProperty(nameof(Lat), "Lat")]
    [QueryProperty(nameof(Lng), "Lng")]
    [QueryProperty(nameof(RestaurantName), "Name")]
    public partial class RestaurantMapPage : ContentPage
    {
        private double _focusLat = 0;
        private double _focusLng = 0;
        private string _focusName = "";
        private CancellationTokenSource? _gpsCts;

        public string Lat
        {
            set
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var v))
                    _focusLat = v;
            }
        }

        public string Lng
        {
            set
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var v))
                    _focusLng = v;
            }
        }

        public string RestaurantName
        {
            set => _focusName = Uri.UnescapeDataString(value ?? "");
        }

        public RestaurantMapPage()
        {
            InitializeComponent();
            mapWebView.Navigated += OnMapLoaded;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var source = new HtmlWebViewSource();
            source.Html = GenerateMapHtml(_focusLat, _focusLng, _focusName);
            mapWebView.Source = source;
            await RequestLocationPermission();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _gpsCts?.Cancel();
        }

        // ✅ Nút quay lại — dùng PopAsync vì đây là route page, không phải tab
        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnMapLoaded(object? sender, WebNavigatedEventArgs e)
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;
            GetLocationOnce();
        }

        private async Task RequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        private async void GetLocationOnce()
        {
            try
            {
                _gpsCts = new CancellationTokenSource();
                var request = new GeolocationRequest(
                    GeolocationAccuracy.Low,
                    TimeSpan.FromSeconds(10)
                );
                var location = await Geolocation.Default.GetLocationAsync(request, _gpsCts.Token);
                if (location != null)
                {
                    var lat = location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var lng = location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var acc = ((int)(location.Accuracy ?? 50)).ToString();
                    await mapWebView.EvaluateJavaScriptAsync($"setLocation({lat}, {lng}, {acc});");
                }
            }
            catch { }
        }

        private string GenerateMapHtml(double lat, double lng, string name)
        {
            var centerLat = lat != 0 ? lat : 10.7583;
            var centerLng = lng != 0 ? lng : 106.7011;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        html, body {{ height: 100%; width: 100%; }}
        #map {{ height: 100vh; width: 100%; }}
    </style>
</head>
<body>
    <div id='map'></div>
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <script>
        var map = L.map('map').setView([{centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {centerLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}], 16);
        L.tileLayer('https://{{s}}.basemaps.cartocdn.com/rastertiles/voyager/{{z}}/{{x}}/{{y}}{{r}}.png', {{
            subdomains: 'abcd', maxZoom: 19
        }}).addTo(map);

        var userMarker = null;
        var userCircle = null;

        function setLocation(lat, lng, accuracy) {{
            if (userMarker) map.removeLayer(userMarker);
            if (userCircle) map.removeLayer(userCircle);
            userMarker = L.circleMarker([lat, lng], {{
                radius: 8, color: '#fff', weight: 3,
                fillColor: '#2196F3', fillOpacity: 1
            }}).addTo(map).bindPopup('📍 Vị trí của bạn');
            userCircle = L.circle([lat, lng], {{
                radius: accuracy, color: '#2196F3',
                fillColor: '#2196F3', fillOpacity: 0.1, weight: 1
            }}).addTo(map);
        }}

        // Marker quán ăn
        var marker = L.marker([{centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {centerLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}]).addTo(map);
        marker.bindPopup('<b>{name}</b>').openPopup();
        L.circle([{centerLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {centerLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}], {{
            color: '#FF5722', fillColor: '#FF5722', fillOpacity: 0.2, radius: 80
        }}).addTo(map);
    </script>
</body>
</html>";
        }
    }
}