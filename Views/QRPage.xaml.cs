using ZXing.Net.Maui;
using DoAn.Services;

namespace DoAn.Views
{
    public partial class QRPage : ContentPage
    {
        public QRPage()
        {
            InitializeComponent();

            // Configure Barcode reader options
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
            barcodeReader.IsDetecting = true;
        }

        private async void BarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();
            if (first == null) return;

            // Stop detecting to avoid multi-scans
            barcodeReader.IsDetecting = false;

            // In this scenario, assume the QR code contains the Restaurant ID (e.g., "1")
            if (int.TryParse(first.Value, out int restaurantId))
            {
                var restaurants = await MockDataService.GetRestaurantsAsync();
                var foundRestaurant = restaurants.FirstOrDefault(r => r.Id == restaurantId);

                if (foundRestaurant != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var navParams = new Dictionary<string, object>
                        {
                            { "Restaurant", foundRestaurant }
                        };
                        await Shell.Current.GoToAsync(nameof(DetailPage), navParams);
                    });
                    return;
                }
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlertAsync("Error", "Invalid QR Code or Restaurant not found.", "OK");
                barcodeReader.IsDetecting = true; // Resume scanning
            });
        }
    }
}