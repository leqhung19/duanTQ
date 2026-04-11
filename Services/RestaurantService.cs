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

        // ⚠️ Đổi thành IP máy tính khi test emulator
        private const string BaseUrl = "http://10.0.2.2:5000/api/";

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
                var result = await _httpClient.GetFromJsonAsync<List<Restaurant>>("restaurants");
                _cache = result ?? new List<Restaurant>();
                return _cache;
            }
            catch
            {
                // Offline — trả về cache
                return _cache.Any() ? _cache : GetMockData();
            }
        }

        public async Task<Restaurant?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Restaurant>($"restaurants/{id}");
            }
            catch
            {
                return _cache.FirstOrDefault(r => r.Id == id);
            }
        }

        // Mock data fallback khi offline
        private List<Restaurant> GetMockData() => new()
        {
            new Restaurant
            {
                Id = 1, CategoryId = 1, Name = "Ốc Oanh",
                Image = "oc_oanh.jpg",
                Description_vi = N("Quán ốc nổi tiếng tại Vĩnh Khánh"),
                Description_en = "Famous seafood restaurant in Vinh Khanh",
                Address = N("123 Vĩnh Khánh, Q4, TP.HCM"),
                Phone = "0901234567", OpenTime = "16:00 - 23:00",
                PriceRange = N("50.000 - 150.000 VND"),
                Latitude = 10.7600, Longitude = 106.7020,
                RadiusMeters = 30, Priority = 1,
                AudioContent_vi = N("Bạn đang đến gần Ốc Oanh, quán ốc nổi tiếng tại Vĩnh Khánh"),
                AudioContent_en = "You are approaching Oc Oanh seafood restaurant"
            },
            new Restaurant
            {
                Id = 2, CategoryId = 2, Name = N("Cơm Tấm Bà Ba"),
                Image = "com_tam.jpg",
                Description_vi = N("Cơm tấm sườn nướng thơm ngon"),
                Description_en = "Grilled pork broken rice",
                Address = N("456 Vĩnh Khánh, Q4, TP.HCM"),
                Phone = "0907654321", OpenTime = "06:00 - 14:00",
                PriceRange = N("30.000 - 60.000 VND"),
                Latitude = 10.7570, Longitude = 106.7000,
                RadiusMeters = 30, Priority = 2,
                AudioContent_vi = N("Bạn đang đến gần Cơm Tấm Bà Ba"),
                AudioContent_en = "You are approaching Com Tam Ba Ba restaurant"
            }
        };

        private static string N(string s) => s;
    }
}