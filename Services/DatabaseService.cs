using SQLite;
using DoAn.FRONTEND.Models;

namespace DoAn.Services
{
    public class DatabaseService
    {
        private static DatabaseService? _instance;
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private SQLiteAsyncConnection? _db;

        private async Task InitAsync()
        {
            if (_db != null) return;

            var dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "doan_local.db3"
            );

            _db = new SQLiteAsyncConnection(dbPath,
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create |
                SQLiteOpenFlags.SharedCache);

            await _db.CreateTableAsync<LocalRestaurant>();
            await _db.CreateTableAsync<LocalQRCode>();

            // Seed dữ liệu nếu chưa có
            await SeedDataAsync();
        }

        // ============ SEED DATA ============
        private async Task SeedDataAsync()
        {
            if (_db == null) return;
            var count = await _db.Table<LocalRestaurant>().CountAsync();
            if (count > 0) return; // Đã có data

            var restaurants = new List<LocalRestaurant>
            {
                new LocalRestaurant
                {
                    Id = 1,
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
                    Latitude = 10.7600, Longitude = 106.7020,
                    RadiusMeters = 30, Priority = 1,
                    AudioContent_vi = "Bạn đang đến gần Ốc Oanh, quán ốc nổi tiếng tại Vĩnh Khánh",
                    AudioContent_en = "You are approaching Oc Oanh seafood restaurant"
                },
                new LocalRestaurant
                {
                    Id = 2,
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
                    Latitude = 10.7570, Longitude = 106.7000,
                    RadiusMeters = 30, Priority = 2,
                    AudioContent_vi = "Bạn đang đến gần Cơm Tấm Bà Ba",
                    AudioContent_en = "You are approaching Com Tam Ba Ba restaurant"
                }
            };

            var qrCodes = new List<LocalQRCode>
            {
                new LocalQRCode { RestaurantId = 1, QRContent = "doan://restaurant/1" },
                new LocalQRCode { RestaurantId = 2, QRContent = "doan://restaurant/2" }
            };

            await _db.InsertAllAsync(restaurants);
            await _db.InsertAllAsync(qrCodes);
        }

        // ============ QUERIES ============

        // Tìm quán theo QR content
        public async Task<LocalRestaurant?> GetByQRContentAsync(string qrContent)
        {
            await InitAsync();

            // Tìm QR code
            var qr = await _db!.Table<LocalQRCode>()
                .Where(q => q.QRContent == qrContent)
                .FirstOrDefaultAsync();

            if (qr == null) return null;

            // Tìm quán tương ứng
            return await _db.Table<LocalRestaurant>()
                .Where(r => r.Id == qr.RestaurantId)
                .FirstOrDefaultAsync();
        }

        // Tìm quán theo ID
        public async Task<LocalRestaurant?> GetByIdAsync(int id)
        {
            await InitAsync();
            return await _db!.Table<LocalRestaurant>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        // Lấy tất cả quán
        public async Task<List<LocalRestaurant>> GetAllAsync()
        {
            await InitAsync();
            return await _db!.Table<LocalRestaurant>().ToListAsync();
        }

        // Sync từ API về SQLite
        public async Task SyncFromApiAsync(List<Restaurant> apiData)
        {
            await InitAsync();

            foreach (var r in apiData)
            {
                var local = new LocalRestaurant
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Image = r.Image,
                    Description_vi = r.Description_vi,
                    Description_en = r.Description_en,
                    Description_kr = r.Description_kr,
                    Description_cn = r.Description_cn,
                    Address = r.Address,
                    Phone = r.Phone,
                    OpenTime = r.OpenTime,
                    PriceRange = r.PriceRange,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    RadiusMeters = r.RadiusMeters,
                    Priority = r.Priority,
                    AudioContent_vi = r.AudioContent_vi,
                    AudioContent_en = r.AudioContent_en,
                    AudioContent_kr = r.AudioContent_kr,
                    AudioContent_cn = r.AudioContent_cn
                };

                // Insert hoặc Update
                await _db!.InsertOrReplaceAsync(local);
            }
        }

        // Thêm QR code mới
        public async Task AddQRCodeAsync(int restaurantId, string qrContent)
        {
            await InitAsync();
            var existing = await _db!.Table<LocalQRCode>()
                .Where(q => q.QRContent == qrContent)
                .FirstOrDefaultAsync();

            if (existing == null)
                await _db.InsertAsync(new LocalQRCode
                {
                    RestaurantId = restaurantId,
                    QRContent = qrContent
                });
        }
    }
}