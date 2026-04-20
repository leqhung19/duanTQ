using DoAn.Services;

namespace DoAn
{
    public partial class App : Application
    {
        [Obsolete]
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
            PresenceService.Instance.Start();

            // Lắng nghe sự kiện POI
            POIService.Instance.OnPOIEntered += async (poi) =>
            {
                await POITriggerService.Instance.TriggerPOIAsync(
                    poi,
                    POIService.Instance.CurrentLang
                );
            };
        }

        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);
            await OpenPoiFromDeepLinkAsync(uri);
        }

        private static async Task OpenPoiFromDeepLinkAsync(Uri uri)
        {
            var local = await DatabaseService.Instance.GetByDeepLinkAsync(uri);
            if (local is null) return;

            var restaurant = local.ToRestaurant();
            restaurant.AudioFiles = (await DatabaseService.Instance.GetAudioFilesAsync(local.Id))
                .Select(a => new DoAn.FRONTEND.Models.RestaurantAudioFile
                {
                    Id = a.Id,
                    Language = a.Language,
                    Url = a.Url,
                    FileSizeBytes = a.FileSizeBytes
                })
                .ToList();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.GoToAsync(nameof(Views.DetailPage),
                    new Dictionary<string, object> { { "Restaurant", restaurant } });

                await POITriggerService.Instance.TriggerPOIAsync(
                    restaurant,
                    POIService.Instance.CurrentLang,
                    "qr");
            });
        }
    }
}
