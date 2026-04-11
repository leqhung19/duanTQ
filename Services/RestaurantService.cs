using System.Net.Http.Json;
using DoAn.FRONTEND.Models;

namespace DoAn.Services
{
    public class RestaurantService
    {
        private readonly HttpClient _httpClient;

        // ⚠️ Đổi IP này thành IP máy tính của bạn khi test trên emulator
        // Emulator Android: dùng 10.0.2.2 thay localhost
        private const string BaseUrl = "http://26.112.166.132:5000/api/";

        public RestaurantService()
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
                return result ?? new List<Restaurant>();
            }
            catch
            {
                // Offline → trả về list rỗng
                return new List<Restaurant>();
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
                return null;
            }
        }
    }
}