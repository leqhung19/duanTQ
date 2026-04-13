using DoAn.FRONTEND.Models;
using DoAn.Services;
using ZXing.Net.Maui;

namespace DoAn.Views
{
    public partial class QRPage : ContentPage
    {
        private bool _isProcessing = false;
        private LocalRestaurant? _scannedRestaurant;

        public QRPage()
        {
            InitializeComponent();

            // Cấu hình scanner
            barcodeReader.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
                AutoRotate = true,
                Multiple = false
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ResetScanner();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isProcessing = false;
        }

        // ============ QUÉT QR ============
        [Obsolete]
        private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
        {
            // Chống quét nhiều lần liên tiếp
            if (_isProcessing) return;
            _isProcessing = true;

            var result = e.Results.FirstOrDefault();
            if (result?.Value == null)
            {
                _isProcessing = false;
                return;
            }

            var qrContent = result.Value;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await ProcessQRCodeAsync(qrContent);
            });
        }

        [Obsolete]
        private async Task ProcessQRCodeAsync(string qrContent)
        {
            // Hiện loading
            ShowLoading(true);

            try
            {
                // Tra cứu từ SQLite — không cần internet, không cần GPS
                var restaurant = await DatabaseService.Instance
                    .GetByQRContentAsync(qrContent);

                ShowLoading(false);

                if (restaurant == null)
                {
                    // Không tìm thấy
                    ShowNotFound();
                    return;
                }

                _scannedRestaurant = restaurant;

                // 1. Rung thông báo
                await VibrateAsync();

                // 2. Hiện panel kết quả
                ShowResultPanel(restaurant);

                // 3. Phát audio ngay lập tức
                await PlayAudioAsync(restaurant);
            }
            catch
            {
                ShowLoading(false);
                ShowNotFound();
            }
        }

        // ============ RUNG ============
        private async Task VibrateAsync()
        {
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
                await Task.Delay(300);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(400));
            }
            catch { }
        }

        // ============ PHÁT AUDIO ============
        private async Task PlayAudioAsync(LocalRestaurant restaurant)
        {
            try
            {
                var lang = POIService.Instance.CurrentLang;
                var text = restaurant.GetAudioContent(lang);

                if (string.IsNullOrEmpty(text)) return;

                var locales = await TextToSpeech.Default.GetLocalesAsync();
                var locale = lang switch
                {
                    "en" => locales.FirstOrDefault(l => l.Language.StartsWith("en")),
                    "ko" => locales.FirstOrDefault(l => l.Language.StartsWith("ko")),
                    "zh" => locales.FirstOrDefault(l => l.Language.StartsWith("zh")),
                    _ => locales.FirstOrDefault(l => l.Language.StartsWith("vi"))
                };

                await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
                {
                    Locale = locale,
                    Volume = 1.0f,
                    Pitch = 1.0f
                });
            }
            catch { }
        }

        // ============ UI HELPERS ============
        private void ShowLoading(bool show)
        {
            LoadingPanel.IsVisible = show;
            NotFoundPanel.IsVisible = false;
            ResultPanel.IsVisible = false;
        }

        private void ShowNotFound()
        {
            NotFoundPanel.IsVisible = true;
            LoadingPanel.IsVisible = false;
            ResultPanel.IsVisible = false;

            // Tự ẩn sau 3 giây và cho quét lại
            Task.Delay(3000).ContinueWith(_ =>
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    NotFoundPanel.IsVisible = false;
                    _isProcessing = false;
                }));
        }

        [Obsolete]
        private void ShowResultPanel(LocalRestaurant restaurant)
        {
            ResultName.Text = restaurant.Name;
            ResultAddress.Text = restaurant.Address ?? "Chưa cập nhật";
            ResultOpenTime.Text = "🕐 " + (restaurant.OpenTime ?? "--");
            ResultPrice.Text = "💰 " + (restaurant.PriceRange ?? "Liên hệ");
            ResultImage.Source = restaurant.Image;

            ResultPanel.IsVisible = true;

            // Animation trượt lên
            ResultPanel.TranslationY = 400;
            ResultPanel.TranslateTo(0, 0, 350, Easing.CubicOut);
        }

        private void ResetScanner()
        {
            _isProcessing = false;
            _scannedRestaurant = null;
            ResultPanel.IsVisible = false;
            NotFoundPanel.IsVisible = false;
            LoadingPanel.IsVisible = false;
        }

        // ============ BUTTON EVENTS ============
        private void OnCloseResult(object? sender, EventArgs e)
        {
            ResultPanel.IsVisible = false;
            _isProcessing = false; // Cho phép quét lại
            _scannedRestaurant = null;
        }

        private void OnRetry(object? sender, EventArgs e)
        {
            NotFoundPanel.IsVisible = false;
            _isProcessing = false;
        }

        private async void OnViewDetail(object? sender, EventArgs e)
        {
            if (_scannedRestaurant == null) return;

            ResultPanel.IsVisible = false;
            var restaurant = _scannedRestaurant.ToRestaurant();

            await Shell.Current.GoToAsync(nameof(DetailPage),
                new Dictionary<string, object> { { "Restaurant", restaurant } });
        }
    }
}