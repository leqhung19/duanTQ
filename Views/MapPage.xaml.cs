using Microsoft.Maui.Devices.Sensors;

namespace DoAn.Views
{
    public partial class MapPage : ContentPage
    {
        private CancellationTokenSource? _gpsCts;

        public MapPage()
        {
            InitializeComponent();
            mapWebView.Navigated += OnMapLoaded;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var source = new HtmlWebViewSource { Html = GenerateMapHtml(10.7583, 106.7011, "") };
            mapWebView.Source = source;
            await RequestLocationPermissionAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _gpsCts?.Cancel();
        }

        private void OnMapLoaded(object? sender, WebNavigatedEventArgs e)
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;
            GetLocationOnce();
        }

        private async Task RequestLocationPermissionAsync()
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
                var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request, _gpsCts.Token);
                if (location != null) await SendLocationToMapAsync(location);
            }
            catch { }
        }

        private async void OnLocateClicked(object? sender, EventArgs e)
        {
            LocateBtn.IsEnabled = false;
            LocateBtn.Text = "⏳";
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(8));
                var location = await Geolocation.Default.GetLocationAsync(request);
                if (location != null)
                    await SendLocationToMapAsync(location);
                else
                    await DisplayAlertAsync("Thông báo", "Không lấy được vị trí", "OK");
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

        private async Task SendLocationToMapAsync(Location location)
        {
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var lat = location.Latitude.ToString(ci);
            var lng = location.Longitude.ToString(ci);
            var acc = ((int)(location.Accuracy ?? 30)).ToString();
            await mapWebView.EvaluateJavaScriptAsync($"setLocationAndZoom({lat},{lng},{acc});");
        }

        private string GenerateMapHtml(double lat, double lng, string name)
        {
            var ci = System.Globalization.CultureInfo.InvariantCulture;
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
  var map=L.map('map').setView([{lat.ToString(ci)},{lng.ToString(ci)}],15);
  L.tileLayer('https://{{s}}.basemaps.cartocdn.com/rastertiles/voyager/{{z}}/{{x}}/{{y}}{{r}}.png',{{
    subdomains:'abcd',maxZoom:19
  }}).addTo(map);
  var userMarker=null,userCircle=null;
  function setLocationAndZoom(lat,lng,accuracy){{
    if(userMarker)map.removeLayer(userMarker);
    if(userCircle)map.removeLayer(userCircle);
    userMarker=L.circleMarker([lat,lng],{{
      radius:8,color:'#fff',weight:3,fillColor:'#2196F3',fillOpacity:1
    }}).addTo(map).bindPopup('📍 Vị trí của bạn').openPopup();
    userCircle=L.circle([lat,lng],{{
      radius:accuracy,color:'#2196F3',fillColor:'#2196F3',fillOpacity:0.1,weight:1
    }}).addTo(map);
    map.flyTo([lat,lng],17,{{animate:true,duration:1.5}});
  }}
</script></body></html>";
        }
    }
}