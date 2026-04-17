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

        private const string BaseUrl = "http://10.0.2.2:5196/api/";

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
                // ✅ Luôn gọi API trước
                var result = await _httpClient
                    .GetFromJsonAsync<List<Restaurant>>("restaurants");

                if (result != null && result.Any())
                {
                    _cache = result;

                    // ✅ Xóa SQLite cũ rồi sync lại toàn bộ
                    await DatabaseService.Instance.ClearAndSyncAsync(_cache);

                    return _cache;
                }
            }
            catch
            {
                // API lỗi → đọc SQLite
            }

            // Offline fallback
            if (_cache.Any()) return _cache;

            var local = await DatabaseService.Instance.GetAllAsync();
            if (local.Any())
                return local.Select(l => l.ToRestaurant()).ToList();

            return GetMockData();
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

        private List<Restaurant> GetMockData() => new()
        {
            new Restaurant { Id = 1, Name = "Ốc Oanh", Image = "oc_oanh.jpg",
                Address = "123 Vĩnh Khánh, Q4, TP.HCM",
                OpenTime = "16:00 - 23:00", PriceRange = "50.000 - 150.000 VND",
                Latitude = 10.7600, Longitude = 106.7020, IsActive = true },
            new Restaurant { Id = 2, Name = "Cơm Tấm Bà Ba", Image = "com_tam.jpg",
                Address = "456 Vĩnh Khánh, Q4, TP.HCM",
                OpenTime = "06:00 - 14:00", PriceRange = "30.000 - 60.000 VND",
                Latitude = 10.7570, Longitude = 106.7000, IsActive = true },
            new Restaurant { Id = 3, Name = "Ốc Bà Tư", Image = "oc_ba_tu.jpg",
                Address = "78 Vĩnh Khánh, Q4, TP.HCM",
                OpenTime = "15:00 - 22:00", PriceRange = "30.000 - 80.000 VND",
                Latitude = 10.7612, Longitude = 106.7035, IsActive = true },
            new Restaurant { Id = 4, Name = "Cơm Tấm Sài Gòn", Image = "com_tam_sg.jpg",
                Address = "210 Vĩnh Khánh, Q4, TP.HCM",
                OpenTime = "05:30 - 13:00", PriceRange = "35.000 - 65.000 VND",
                Latitude = 10.7558, Longitude = 106.6988, IsActive = true },
            new Restaurant { Id = 5, Name = "Bún Bò Huế Dì Sáu", Image = "bun_bo_hue.jpg",
                Address = "95 Vĩnh Khánh, Q4, TP.HCM",
                OpenTime = "06:00 - 11:00", PriceRange = "40.000 - 70.000 VND",
                Latitude = 10.7595, Longitude = 106.7008, IsActive = true }
        };
    }
}