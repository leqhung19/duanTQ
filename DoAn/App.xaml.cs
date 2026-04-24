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
            _ = PresenceService.Instance.RefreshAsync();

            // Lắng nghe sự kiện POI
            POIService.Instance.OnPOIEntered += async (poi) =>
            {
                await POITriggerService.Instance.TriggerPOIAsync(
                    poi,
                    POIService.Instance.CurrentLang
                );
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            window.Activated += (_, _) =>
            {
                PresenceService.Instance.Start();
                _ = PresenceService.Instance.RefreshAsync();
            };

            window.Resumed += (_, _) =>
            {
                PresenceService.Instance.Start();
                _ = PresenceService.Instance.RefreshAsync();
            };

            return window;
        }

        protected override async void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);
            await OpenPoiFromDeepLinkAsync(uri);
        }

        private static async Task OpenPoiFromDeepLinkAsync(Uri uri)
        {
            var apiBaseUrl = GetQueryValue(uri, "api");
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                RestaurantService.Instance.SetPreferredBaseUrl(apiBaseUrl);
                PresenceService.Instance.SetPreferredBaseUrl(apiBaseUrl);
            }

            var restaurant = TryGetPoiId(uri, out var poiId)
                ? await RestaurantService.Instance.GetByIdAsync(poiId)
                : null;

            if (restaurant is null)
            {
                var local = await DatabaseService.Instance.GetByDeepLinkAsync(uri);
                if (local is null) return;

                restaurant = local.ToRestaurant();
                restaurant.AudioFiles = (await DatabaseService.Instance.GetAudioFilesAsync(local.Id))
                    .Select(a => new DoAn.FRONTEND.Models.RestaurantAudioFile
                    {
                        Id = a.Id,
                        Language = a.Language,
                        Url = a.Url,
                        FileSizeBytes = a.FileSizeBytes
                    })
                    .ToList();
            }

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

        private static bool TryGetPoiId(Uri uri, out int poiId)
        {
            poiId = 0;

            if (string.Equals(uri.Scheme, "doan", StringComparison.OrdinalIgnoreCase)
                && string.Equals(uri.Host, "restaurant", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(uri.AbsolutePath.Trim('/'), out poiId);
            }

            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 2
                && string.Equals(segments[0], "q", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(segments[1], out poiId);
        }

        private static string? GetQueryValue(Uri uri, string key)
        {
            var query = uri.Query.TrimStart('?');
            if (string.IsNullOrWhiteSpace(query)) return null;

            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split('=', 2);
                if (pair.Length == 2 && string.Equals(pair[0], key, StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(pair[1].Replace("+", " "));
            }

            return null;
        }
    }
}
