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
                var result = await _httpClient
                    .GetFromJsonAsync<List<Restaurant>>("restaurants");

                _cache = result ?? new List<Restaurant>();

                // Sync về SQLite
                await DatabaseService.Instance.SyncFromApiAsync(_cache);

                return _cache;
            }
            catch
            {
                // Offline → SQLite
                if (_cache.Any()) return _cache;

                var local = await DatabaseService.Instance.GetAllAsync();
                if (local.Any())
                    return local.Select(l => l.ToRestaurant()).ToList();

                // Fallback mock data 5 quán
                return GetMockData();
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

        // ✅ Mock data 5 quán đầy đủ
        private List<Restaurant> GetMockData() => new()
        {
            new Restaurant
            {
                Id = 1,
                CategoryId = 1,
                Name = "Ốc Oanh",
                Image = "oc_oanh.jpg",
                Description_vi = "Quán ốc nổi tiếng tại Vĩnh Khánh",
                Description_en = "Famous seafood restaurant in Vinh Khanh",
                Description_kr = "빈칸의 유명한 해산물 식당",
                Description_cn = "永庆著名的海鲜餐厅",
                Address = "123 Vĩnh Khánh, Q4, TP.HCM",
                Phone = "0901234567",
                OpenTime = "16:00 - 23:00",
                PriceRange = "50.000 - 150.000 VND",
                Latitude = 10.7600,
                Longitude = 106.7020,
                RadiusMeters = 30,
                Priority = 1,
                AudioContent_vi = "Bạn đang đến gần Ốc Oanh, quán ốc nổi tiếng tại Vĩnh Khánh",
                AudioContent_en = "You are approaching Oc Oanh seafood restaurant",
                IsActive = true
            },
            new Restaurant
            {
                Id = 2,
                CategoryId = 2,
                Name = "Cơm Tấm Bà Ba",
                Image = "com_tam.jpg",
                Description_vi = "Cơm tấm sườn nướng thơm ngon",
                Description_en = "Grilled pork broken rice",
                Description_kr = "구운 돼지고기 분쌀",
                Description_cn = "烤猪肉碎米饭",
                Address = "456 Vĩnh Khánh, Q4, TP.HCM",
                Phone = "0907654321",
                OpenTime = "06:00 - 14:00",
                PriceRange = "30.000 - 60.000 VND",
                Latitude = 10.7570,
                Longitude = 106.7000,
                RadiusMeters = 30,
                Priority = 2,
                AudioContent_vi = "Bạn đang đến gần Cơm Tấm Bà Ba",
                AudioContent_en = "You are approaching Com Tam Ba Ba restaurant",
                IsActive = true
            },
            new Restaurant
            {
                Id = 15,
                CategoryId = 1,
                Name = "Ốc Bà Tư",
                Image = "oc_ba_tu.jpg",
                Description_vi = "Quán ốc bình dân giá rẻ, đông khách",
                Description_en = "Affordable seafood restaurant, always crowded",
                Description_kr = "저렴한 해산물 식당",
                Description_cn = "价格实惠的海鲜餐厅",
                Address = "78 Vĩnh Khánh, Q4, TP.HCM",
                Phone = "0912345678",
                OpenTime = "15:00 - 22:00",
                PriceRange = "30.000 - 80.000 VND",
                Latitude = 10.7612,
                Longitude = 106.7035,
                RadiusMeters = 30,
                Priority = 3,
                AudioContent_vi = "Bạn đang đến gần Ốc Bà Tư, quán ốc bình dân nổi tiếng",
                AudioContent_en = "You are approaching Oc Ba Tu restaurant",
                IsActive = true
            },
            new Restaurant
            {
                Id = 16,
                CategoryId = 2,
                Name = "Cơm Tấm Sài Gòn",
                Image = "com_tam_sg.jpg",
                Description_vi = "Cơm tấm đặc biệt với sườn nướng và chả trứng",
                Description_en = "Special broken rice with grilled pork and egg",
                Description_kr = "특별한 분쌀 요리",
                Description_cn = "特别碎米饭套餐",
                Address = "210 Vĩnh Khánh, Q4, TP.HCM",
                Phone = "0923456789",
                OpenTime = "05:30 - 13:00",
                PriceRange = "35.000 - 65.000 VND",
                Latitude = 10.7558,
                Longitude = 106.6988,
                RadiusMeters = 30,
                Priority = 4,
                AudioContent_vi = "Bạn đang đến gần Cơm Tấm Sài Gòn",
                AudioContent_en = "You are approaching Com Tam Sai Gon restaurant",
                IsActive = true
            },
            new Restaurant
            {
                Id = 17,
                CategoryId = 3,
                Name = "Bún Bò Huế Dì Sáu",
                Image = "bun_bo_hue.jpg",
                Description_vi = "Bún bò Huế chuẩn vị miền Trung, nước dùng đậm đà",
                Description_en = "Authentic Hue beef noodle soup with rich broth",
                Description_kr = "정통 후에 소고기 쌀국수",
                Description_cn = "正宗顺化牛肉粉",
                Address = "95 Vĩnh Khánh, Q4, TP.HCM",
                Phone = "0934567890",
                OpenTime = "06:00 - 11:00",
                PriceRange = "40.000 - 70.000 VND",
                Latitude = 10.7595,
                Longitude = 106.7008,
                RadiusMeters = 30,
                Priority = 5,
                AudioContent_vi = "Bạn đang đến gần Bún Bò Huế Dì Sáu, đặc sản miền Trung",
                AudioContent_en = "You are approaching Bun Bo Hue Di Sau restaurant",
                IsActive = true
            }
        };
    }
}