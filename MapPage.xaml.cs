using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace DoAn.Views
{
    [QueryProperty(nameof(Lat), "Lat")]
    [QueryProperty(nameof(Lng), "Lng")]
    [QueryProperty(nameof(RestaurantName), "Name")]
    public partial class MapPage : ContentPage
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

        public MapPage()
        {
            InitializeComponent();
            mapWebView.Navigated += OnMapLoaded;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load map từ file offline
            mapWebView.Source = new UrlWebViewSource
            {
                Url = "offline_map.html"
            };

            // Xin permission GPS
            await RequestLocationPermission();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // ✅ Tiết kiệm pin — hủy GPS khi rời trang
            _gpsCts?.Cancel();
        }

        private void OnMapLoaded(object? sender, WebNavigatedEventArgs e)
        {
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;

            // Nếu được gọi từ DetailPage — focus vào quán
            if (_focusLat != 0 && _focusLng != 0)
            {
                var js = $"focusRestaurant({_focusLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {_focusLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{_focusName}');";
                mapWebView.EvaluateJavaScriptAsync(js);
            }
            else
            {
                // Tự động lấy GPS khi mở map
                GetLocationOnce();
            }
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

                // ✅ Tiết kiệm pin: dùng Low accuracy (mạng/wifi thay GPS)
                // Chuyển sang Medium/Best nếu cần chính xác hơn
                var request = new GeolocationRequest(
                    GeolocationAccuracy.Low,        // Tiết kiệm pin
                    TimeSpan.FromSeconds(10)         // Timeout 10 giây
                );

                var location = await Geolocation.Default.GetLocationAsync(request, _gpsCts.Token);

                if (location != null)
                {
                    var lat = location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var lng = location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var acc = ((int)(location.Accuracy ?? 50)).ToString();

                    // Gửi tọa độ vào WebView
                    await mapWebView.EvaluateJavaScriptAsync(
                        $"setLocation({lat}, {lng}, {acc});"
                    );
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlertAsync("Thông báo", "Thiết bị không hỗ trợ GPS", "OK");
            }
            catch (PermissionException)
            {
                await DisplayAlertAsync("Thông báo", "Vui lòng cấp quyền vị trí", "OK");
            }
            catch (Exception)
            {
                // Offline — bỏ qua, map vẫn hiện bình thường
            }
        }
        private async void OnBackClicked(object? sender, EventArgs e)
        {
            try
            {
                // Thử cách 1
                await Navigation.PopAsync();
            }
            catch
            {
                try
                {
                    // Thử cách 2
                    await Shell.Current.GoToAsync("..");
                }
                catch
                {
                    // Thử cách 3
                    await Shell.Current.Navigation.PopAsync();
                }
            }
        }
    }
}
