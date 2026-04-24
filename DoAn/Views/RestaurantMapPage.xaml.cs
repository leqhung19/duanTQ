using Microsoft.Maui.Devices.Sensors;

namespace DoAn.Views
{
    [QueryProperty(nameof(Lat), "Lat")]
    [QueryProperty(nameof(Lng), "Lng")]
    [QueryProperty(nameof(RestaurantName), "Name")]
    public partial class RestaurantMapPage : ContentPage
    {
        private double _lat = 10.7583;
        private double _lng = 106.7011;
        private string _name = "";
        private CancellationTokenSource? _gpsCts;

        public string Lat
        {
            set
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var v)) _lat = v;
            }
        }
        public string Lng
        {
            set
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var v)) _lng = v;
            }
        }
        public string RestaurantName
        {
            set => _name = Uri.UnescapeDataString(value ?? "");
        }

        public RestaurantMapPage()
        {
            InitializeComponent();
            mapWebView.Navigated += OnMapLoaded;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            mapWebView.Source = new HtmlWebViewSource { Html = GenerateMapHtml() };
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _gpsCts?.Cancel();
        }

        private async void OnMapLoaded(object? sender, WebNavigatedEventArgs e)
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;

            // ✅ Map mở ra focus vào QUÁN trước
            // Sau đó mới lấy vị trí người dùng hiển thị phụ
            await GetUserLocationSilentlyAsync();
        }

        private async void OnBackClicked(object? sender, EventArgs e)
            => await Navigation.PopAsync();

        private async void OnLocateClicked(object? sender, EventArgs e)
        {
            LocateBtn.IsEnabled = false;
            LocateBtn.Text = "⏳";
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(8));
                var location = await Geolocation.Default.GetLocationAsync(request);
                if (location != null)
                {
                    // Nút locate thì mới zoom về vị trí người dùng
                    await SendUserLocationAsync(location, zoomToUser: true);
                }
            }
            catch { }
            finally { LocateBtn.IsEnabled = true; LocateBtn.Text = "📍"; }
        }

        // ✅ Lấy vị trí im lặng — chỉ vẽ dot, KHÔNG zoom
        private async Task GetUserLocationSilentlyAsync()
        {
            try
            {
                _gpsCts = new CancellationTokenSource();
                var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request, _gpsCts.Token);
                if (location != null)
                    await SendUserLocationAsync(location, zoomToUser: false);
            }
            catch { }
        }

        private async Task SendUserLocationAsync(Location location, bool zoomToUser)
        {
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var lat = location.Latitude.ToString(ci);
            var lng = location.Longitude.ToString(ci);
            var acc = ((int)(location.Accuracy ?? 30)).ToString();

            if (zoomToUser)
                // Nhấn nút locate → zoom về người dùng
                await mapWebView.EvaluateJavaScriptAsync($"setUserLocationAndZoom({lat},{lng},{acc});");
            else
                // Load xong → chỉ vẽ dot, giữ nguyên focus quán
                await mapWebView.EvaluateJavaScriptAsync($"setUserLocationOnly({lat},{lng},{acc});");
        }

        private string GenerateMapHtml()
        {
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var latStr = _lat.ToString(ci);
            var lngStr = _lng.ToString(ci);

            return $@"
<!DOCTYPE html><html><head>
<meta charset='utf-8'/>
<meta name='viewport' content='width=device-width,initial-scale=1.0'>
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<style>
  *{{margin:0;padding:0;box-sizing:border-box}}
  html,body{{height:100%;width:100%}}
  #map{{height:100vh;width:100%}}
</style></head><body>
<div id='map'></div>
<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<script>
  // ✅ Focus vào QUÁN khi mở
  var map = L.map('map').setView([{latStr},{lngStr}], 17);

  L.tileLayer('https://{{s}}.basemaps.cartocdn.com/rastertiles/voyager/{{z}}/{{x}}/{{y}}{{r}}.png',{{
    subdomains:'abcd', maxZoom:19
  }}).addTo(map);

  // Marker quán — luôn hiển thị
  L.marker([{latStr},{lngStr}]).addTo(map)
    .bindPopup('<b>{_name}</b>').openPopup();
  L.circle([{latStr},{lngStr}],{{
    color:'#FF5722', fillColor:'#FF5722', fillOpacity:0.15, radius:50, weight:2
  }}).addTo(map);

  var userMarker = null, userCircle = null;

  // Chỉ vẽ dot người dùng, KHÔNG zoom
  function setUserLocationOnly(lat, lng, accuracy) {{
    if(userMarker) map.removeLayer(userMarker);
    if(userCircle) map.removeLayer(userCircle);
    userMarker = L.circleMarker([lat,lng], {{
      radius:8, color:'#fff', weight:3, fillColor:'#2196F3', fillOpacity:1
    }}).addTo(map).bindPopup('📍 Vị trí của bạn');
    userCircle = L.circle([lat,lng], {{
      radius:accuracy, color:'#2196F3', fillColor:'#2196F3', fillOpacity:0.1, weight:1
    }}).addTo(map);
    // KHÔNG gọi map.flyTo ở đây
  }}

  // Nhấn nút locate → zoom về người dùng
  function setUserLocationAndZoom(lat, lng, accuracy) {{
    setUserLocationOnly(lat, lng, accuracy);
    map.flyTo([lat,lng], 17, {{animate:true, duration:1.5}});
  }}
</script></body></html>";
        }
    }
}