using Microsoft.Maui.Controls;

namespace DoAn.Views
{
    [QueryProperty(nameof(Lat), "Lat")]
    [QueryProperty(nameof(Lng), "Lng")]
    [QueryProperty(nameof(RestaurantName), "Name")]
    public partial class MapPage : ContentPage
    {
        private double _lat = 10.7583; // Tọa độ mặc định
        private double _lng = 106.7011;
        private string _name = "";

        public string Lat
        {
            set
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var lat))
                    _lat = lat;
            }
        }

        public string Lng
        {
            set
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var lng))
                    _lng = lng;
            }
        }

        public string RestaurantName
        {
            set => _name = Uri.UnescapeDataString(value ?? "");
        }

        public MapPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadMap();
        }

        private void LoadMap()
        {
            var html = GenerateMapHtml(_lat, _lng, _name);
            var source = new HtmlWebViewSource();
            source.Html = html;
            mapWebView.Source = source;
        }

        private string GenerateMapHtml(double lat, double lng, string name)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bản đồ</title>
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
        var lat = {lat.ToString(System.Globalization.CultureInfo.InvariantCulture)};
        var lng = {lng.ToString(System.Globalization.CultureInfo.InvariantCulture)};
        var name = '{name}';

        var map = L.map('map').setView([lat, lng], 16);

        L.tileLayer('https://{{s}}.basemaps.cartocdn.com/rastertiles/voyager/{{z}}/{{x}}/{{y}}{{r}}.png', {{
            attribution: '© OpenStreetMap © CARTO',
            subdomains: 'abcd',
            maxZoom: 19
        }}).addTo(map);

        // Marker trỏ đúng vào quán
        var marker = L.marker([lat, lng]).addTo(map);
        marker.bindPopup('<b>' + name + '</b>').openPopup();

        // Vòng tròn highlight
        L.circle([lat, lng], {{
            color: '#FF5722',
            fillColor: '#FF5722',
            fillOpacity: 0.2,
            radius: 100
        }}).addTo(map);
    </script>
</body>
</html>";
        }
    }
}