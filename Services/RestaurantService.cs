using System.Net.Http.Json;
using DoAn.FRONTEND.Models;
using DoAn.Services;

namespace DoAn.Services
{
    public class RestaurantService
    {
        private static RestaurantService? _instance;
        public static RestaurantService Instance => _instance ??= new RestaurantService();

        private readonly HttpClient _httpClient;
        private List<Restaurant> _cache = new();

        // ⚠️ QUAN TRỌNG:
        // Emulator Android  → dùng http://10.0.2.2:5000/api/
        // Thiết bị thật     → dùng IP máy tính VD: http://192.168.1.5:5000/api/
        private const string BaseUrl = "http://192.168.1.6:5196/api/";

        private RestaurantService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<List<Restaurant>> GetAllAsync()
        {
            try
            {
                var result = await _httpClient
                    .GetFromJsonAsync<List<Restaurant>>("restaurants");

                _cache = result ?? new List<Restaurant>();

                // Sync về SQLite để dùng offline
                await DatabaseService.Instance.SyncFromApiAsync(_cache);

                return _cache;
            }
            catch
            {
                // Offline → đọc từ SQLite
                if (_cache.Any()) return _cache;

                var local = await DatabaseService.Instance.GetAllAsync();
                return local.Select(l => l.ToRestaurant()).ToList();
            }
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient
                    .GetFromJsonAsync<Restaurant>($"restaurants/{id}");
            }
            catch
            {
                var local = await DatabaseService.Instance.GetByIdAsync(id);
                return local?.ToRestaurant();
            }
        }
    }
}