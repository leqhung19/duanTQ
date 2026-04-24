using System.Net.Http.Json;
using DoAn.FRONTEND.Models;

namespace DoAn.Services
{
    public class RestaurantService
    {
        private static RestaurantService? _instance;
        public static RestaurantService Instance => _instance ??= new RestaurantService();

        private readonly HttpClient _httpClient;
        private List<Restaurant> _cache = new();
        private string _activeBaseUrl;

        // Thu tu uu tien:
        // 10.0.2.2: Android emulator goi web admin tren cung may.
        // localhost: chay app Windows cung may web admin.
        // IP LAN: dien thoai that cung Wi-Fi voi may chay web admin.
        private static readonly string[] BaseUrls =
        [
            "http://vinh-khanh.somee.com/api/",
            "http://10.0.2.2:5143/api/",
            "http://localhost:5143/api/",
            "http://10.93.119.86:5143/api/"
        ];

        private RestaurantService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            _activeBaseUrl = NormalizeBaseUrl(Preferences.Get("ApiBaseUrl", "http://vinh-khanh.somee.com/api/"));
        }

        public void SetPreferredBaseUrl(string? apiBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl)) return;

            _activeBaseUrl = NormalizeBaseUrl(apiBaseUrl);
            Preferences.Set("ApiBaseUrl", _activeBaseUrl);
        }

        public async Task<List<Restaurant>> GetAllAsync()
        {
            try
            {
                var sync = await GetSyncDataAsync();
                var result = sync?.Pois
                    .Select(p => p.ToRestaurant())
                    .Where(r => r.IsActive)
                    .OrderByDescending(r => r.Priority)
                    .ToList();

                if (result is { Count: > 0 })
                {
                    System.Diagnostics.Debug.WriteLine(">>> Lay tu API thanh cong: " + result.Count + " POI");
                    _cache = result;
                    await DatabaseService.Instance.ClearAndSyncAsync(_cache);
                    return _cache;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(">>> Loi API: " + ex.Message);
            }

            if (_cache.Any()) return _cache;

            var local = await DatabaseService.Instance.GetAllAsync();
            if (local.Any()) return await MapLocalAsync(local);

            return GetMockData();
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            try
            {
                var sync = await GetSyncDataAsync();
                return sync?.Pois.FirstOrDefault(p => p.Id == id)?.ToRestaurant();
            }
            catch
            {
                var local = await DatabaseService.Instance.GetByIdAsync(id);
                if (local is null) return null;

                var mapped = await MapLocalAsync(new List<LocalRestaurant> { local });
                return mapped.FirstOrDefault();
            }
        }

        public async Task LogListenAsync(int restaurantId, string language, string triggerType, string audioSource)
        {
            try
            {
                await _httpClient.PostAsJsonAsync(_activeBaseUrl + "sync/log", new
                {
                    restaurantId,
                    language = ToServerLanguage(language),
                    audioSource,
                    triggerType,
                    sessionId = PresenceService.Instance.SessionId
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(">>> Loi ghi log nghe: " + ex.Message);
            }
        }

        private async Task<SyncResponse?> GetSyncDataAsync()
        {
            foreach (var baseUrl in BaseUrls.Prepend(_activeBaseUrl).Distinct())
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine(">>> Dang goi API: " + baseUrl + "sync/pois");
                    var sync = await _httpClient.GetFromJsonAsync<SyncResponse>(baseUrl + "sync/pois");
                    if (sync is not null)
                    {
                        _activeBaseUrl = baseUrl;
                        return sync;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(">>> Loi API " + baseUrl + ": " + ex.Message);
                }
            }

            return null;
        }

        private async Task<List<Restaurant>> MapLocalAsync(List<LocalRestaurant> locals)
        {
            var result = new List<Restaurant>();

            foreach (var local in locals)
            {
                var restaurant = local.ToRestaurant();
                restaurant.AudioFiles = (await DatabaseService.Instance.GetAudioFilesAsync(local.Id))
                    .Select(a => new RestaurantAudioFile
                    {
                        Id = a.Id,
                        Language = ToAppLanguage(a.Language),
                        Url = a.Url,
                        FileSizeBytes = a.FileSizeBytes
                    })
                    .ToList();
                result.Add(restaurant);
            }

            return result.OrderByDescending(r => r.Priority).ToList();
        }

        private static string ToServerLanguage(string language) => language switch
        {
            "zh" => "cn",
            "ko" => "ko",
            "en" => "en",
            _ => "vi"
        };

        private static string ToAppLanguage(string? language) => language?.ToLowerInvariant() switch
        {
            "cn" or "zh" or "zh-cn" => "zh",
            "kr" or "ko" => "ko",
            "en" => "en",
            _ => "vi"
        };

        private static string NormalizeBaseUrl(string value)
        {
            var normalized = value.Trim();
            if (!normalized.EndsWith('/'))
                normalized += "/";

            return normalized;
        }

        private List<Restaurant> GetMockData() => new()
        {
            new Restaurant { Id = 1, Name = "Oc Oanh", Image = "oc_oanh.jpg",
                Address = "123 Vinh Khanh, Q4, TP.HCM",
                OpenTime = "16:00 - 23:00", PriceRange = "50.000 - 150.000 VND",
                Latitude = 10.7600, Longitude = 106.7020, IsActive = true },
            new Restaurant { Id = 2, Name = "Com Tam Ba Ba", Image = "com_tam.jpg",
                Address = "456 Vinh Khanh, Q4, TP.HCM",
                OpenTime = "06:00 - 14:00", PriceRange = "30.000 - 60.000 VND",
                Latitude = 10.7570, Longitude = 106.7000, IsActive = true },
            new Restaurant { Id = 3, Name = "Oc Ba Tu", Image = "oc_ba_tu.jpg",
                Address = "78 Vinh Khanh, Q4, TP.HCM",
                OpenTime = "15:00 - 22:00", PriceRange = "30.000 - 80.000 VND",
                Latitude = 10.7612, Longitude = 106.7035, IsActive = true },
            new Restaurant { Id = 4, Name = "Com Tam Sai Gon", Image = "com_tam_sg.jpg",
                Address = "210 Vinh Khanh, Q4, TP.HCM",
                OpenTime = "05:30 - 13:00", PriceRange = "35.000 - 65.000 VND",
                Latitude = 10.7558, Longitude = 106.6988, IsActive = true },
            new Restaurant { Id = 5, Name = "Bun Bo Hue Di Sau", Image = "bun_bo_hue.jpg",
                Address = "95 Vinh Khanh, Q4, TP.HCM",
                OpenTime = "06:00 - 11:00", PriceRange = "40.000 - 70.000 VND",
                Latitude = 10.7595, Longitude = 106.7008, IsActive = true }
        };
    }
}
