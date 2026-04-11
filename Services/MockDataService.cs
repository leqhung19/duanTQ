using System.Text.Json;
using DoAn.FRONTEND.Models;

namespace DoAn.Services
{
    public static class MockDataService
    {
        public static async Task<List<Restaurant>> GetRestaurantsAsync()
        {
            // Simulate API network delay
            await Task.Delay(500);

            return new List<Restaurant>
            {
                new Restaurant
                {
                    Id = 1,
                    Name = "Ốc Oanh",
                    Image = "https://images.unsplash.com/photo-1559742811-822873691df8?w=500",
                    Description_vi = "Quán ốc nổi tiếng nhất Vĩnh Khánh với hải sản tươi sống và sốt mắm chanh tuyệt đỉnh.",
                    Description_en = "The most famous snail restaurant on Vinh Khanh with fresh seafood and excellent lime fish sauce.",
                    Description_kr = "빈칸 거리에서 가장 유명한 달팽이 요리 전문점입니다.",
                    Description_cn = "永庆街最著名的螺蛳店。",
                    Latitude = 10.7600,
                    Longitude = 106.7000
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Sườn Nướng BBQ",
                    Image = "https://images.unsplash.com/photo-1544025162-d76694265947?w=500",
                    Description_vi = "Sườn nướng than hoa thơm lừng, thịt mềm tan trong miệng.",
                    Description_en = "Charcoal-grilled ribs that are fragrant and melt-in-your-mouth tender.",
                    Latitude = 10.7595,
                    Longitude = 106.7010
                },
                new Restaurant
                {
                    Id = 2,
                    Name = "Sườn Nướng BBQ",
                    Image = "https://images.unsplash.com/photo-1544025162-d76694265947?w=500",
                    Description_vi = "Sườn nướng than hoa thơm lừng, thịt mềm tan trong miệng.",
                    Description_en = "Charcoal-grilled ribs that are fragrant and melt-in-your-mouth tender.",
                    Latitude = 10.7595,
                    Longitude = 106.7010
                }
            };
        }
    }
}