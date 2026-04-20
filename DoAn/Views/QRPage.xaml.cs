using DoAn.FRONTEND.Models;
using DoAn.Services;
using ZXing;
using ZXing.Net.Maui;

namespace DoAn.Views
{
    public partial class QRPage : ContentPage
    {
        private bool _isProcessing;
        private LocalRestaurant? _scannedRestaurant;

        public QRPage()
        {
            InitializeComponent();

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

        private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
        {
            if (_isProcessing) return;
            _isProcessing = true;

            var result = e.Results.FirstOrDefault();
            if (result?.Value == null)
            {
                _isProcessing = false;
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
                await ProcessQRCodeAsync(result.Value));
        }

        [Obsolete]
        private async Task ProcessQRCodeAsync(string qrContent)
        {
            ShowLoading(true);

            try
            {
                var restaurant = await DatabaseService.Instance.GetByQRContentAsync(qrContent);
                ShowLoading(false);

                if (restaurant == null)
                {
                    ShowNotFound();
                    return;
                }

                _scannedRestaurant = restaurant;
                ShowResultPanel(restaurant);

                var audioFiles = await DatabaseService.Instance.GetAudioFilesAsync(restaurant.Id);
                var poi = restaurant.ToRestaurant();
                poi.AudioFiles = audioFiles.Select(a => new RestaurantAudioFile
                {
                    Id = a.Id,
                    Language = a.Language,
                    Url = a.Url,
                    FileSizeBytes = a.FileSizeBytes
                }).ToList();

                await POITriggerService.Instance.TriggerPOIAsync(
                    poi,
                    POIService.Instance.CurrentLang,
                    "qr");
            }
            catch
            {
                ShowLoading(false);
                ShowNotFound();
            }
        }

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
            ResultAddress.Text = restaurant.Address ?? "Chua cap nhat";
            ResultOpenTime.Text = "Gio: " + (restaurant.OpenTime ?? "--");
            ResultPrice.Text = "Gia: " + (restaurant.PriceRange ?? "Lien he");
            ResultImage.Source = restaurant.Image;

            ResultPanel.IsVisible = true;
            ResultPanel.TranslationY = 400;
            _ = ResultPanel.TranslateTo(0, 0, 350, Easing.CubicOut);
        }

        private void ResetScanner()
        {
            _isProcessing = false;
            _scannedRestaurant = null;
            ResultPanel.IsVisible = false;
            NotFoundPanel.IsVisible = false;
            LoadingPanel.IsVisible = false;
        }

        private void OnCloseResult(object? sender, EventArgs e)
        {
            ResultPanel.IsVisible = false;
            _isProcessing = false;
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
            restaurant.AudioFiles = (await DatabaseService.Instance.GetAudioFilesAsync(restaurant.Id))
                .Select(a => new RestaurantAudioFile
                {
                    Id = a.Id,
                    Language = a.Language,
                    Url = a.Url,
                    FileSizeBytes = a.FileSizeBytes
                })
                .ToList();

            await Shell.Current.GoToAsync(nameof(DetailPage),
                new Dictionary<string, object> { { "Restaurant", restaurant } });
        }
    }
}
