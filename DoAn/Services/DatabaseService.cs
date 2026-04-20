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
            await _db.CreateTableAsync<LocalAudioFile>();

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
                },
                new LocalRestaurant
                {
                Id = 3,
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
                },
                new LocalRestaurant
                {
                Id = 4,
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
                },
                new LocalRestaurant
                {
                Id = 5,
                Name = "Bun Bo Hue Di Sau",
                Image = "bun_bo_hue.jpg",
                Description_vi = "Bun bo Hue chuan vi mien Trung, nuoc dung dam da",
                Description_en = "Authentic Hue beef noodle soup with rich broth",
                Description_kr = "정통 후에 소고기 쌀국수",
                Description_cn = "正宗顺化牛肉粉",
                Address = "95 Vinh Khanh, Q4, TP.HCM",
                Phone = "0934567890",
                OpenTime = "06:00 - 11:00",
                PriceRange = "40.000 - 70.000 VND",
                Latitude = 10.7595,
                Longitude = 106.7008,
                RadiusMeters = 30,
                Priority = 5,
                AudioContent_vi = "Ban dang den gan Bun Bo Hue Di Sau, dac san mien Trung",
                AudioContent_en = "You are approaching Bun Bo Hue Di Sau restaurant",
                AudioContent_kr = "당신은 분보후에 디사우 음식점에 접근하고 있습니다",
                AudioContent_cn = "你正在接近Bun Bo Hue Di Sau餐厅",
                },
            };

            var qrCodes = new List<LocalQRCode>
            {
                new LocalQRCode { RestaurantId = 1, QRContent = "doan://restaurant/1" },
                new LocalQRCode { RestaurantId = 2, QRContent = "doan://restaurant/2" },
                new LocalQRCode { RestaurantId = 3, QRContent = "doan://restaurant/3" },
                new LocalQRCode { RestaurantId = 4, QRContent = "doan://restaurant/4" },
                new LocalQRCode { RestaurantId = 5, QRContent = "doan://restaurant/5" }
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

            if (qr == null && TryGetPoiIdFromQr(qrContent, out var parsedPoiId))
            {
                return await _db.Table<LocalRestaurant>()
                    .Where(r => r.Id == parsedPoiId)
                    .FirstOrDefaultAsync();
            }

            if (qr == null) return null;

            // Tìm quán tương ứng
            return await _db.Table<LocalRestaurant>()
                .Where(r => r.Id == qr.RestaurantId)
                .FirstOrDefaultAsync();
        }

        public async Task<LocalRestaurant?> GetByDeepLinkAsync(Uri uri)
        {
            await InitAsync();

            if (!TryGetPoiIdFromQr(uri.ToString(), out var poiId))
                return null;

            return await _db!.Table<LocalRestaurant>()
                .Where(r => r.Id == poiId)
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

        public async Task<List<LocalAudioFile>> GetAudioFilesAsync(int restaurantId)
        {
            await InitAsync();
            return await _db!.Table<LocalAudioFile>()
                .Where(a => a.RestaurantId == restaurantId)
                .ToListAsync();
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
        // ✅ Xóa toàn bộ data cũ rồi sync lại
        public async Task ClearAndSyncAsync(List<Restaurant> apiData)
        {
            await InitAsync();

            // Xóa hết data cũ
            await _db!.DeleteAllAsync<LocalRestaurant>();
            await _db.DeleteAllAsync<LocalQRCode>();
            await _db.DeleteAllAsync<LocalAudioFile>();

            // Insert lại toàn bộ từ API
            var locals = apiData.Select(r => new LocalRestaurant
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
            }).ToList();

            await _db.InsertAllAsync(locals);

            var qrCodes = apiData
                .SelectMany(r => r.QrCodes.Select(q => new LocalQRCode
                {
                    RestaurantId = r.Id,
                    QRContent = q
                }))
                .ToList();

            if (qrCodes.Any())
                await _db.InsertAllAsync(qrCodes);

            var audioFiles = apiData
                .SelectMany(r => r.AudioFiles
                    .Where(a => !string.IsNullOrWhiteSpace(a.Url))
                    .Select(a => new LocalAudioFile
                    {
                        Id = a.Id,
                        RestaurantId = r.Id,
                        Language = NormalizeLanguage(a.Language),
                        Url = a.Url!,
                        FileSizeBytes = a.FileSizeBytes
                    }))
                .ToList();

            if (audioFiles.Any())
                await _db.InsertAllAsync(audioFiles);
        }

        private static string NormalizeLanguage(string? language) => language?.ToLowerInvariant() switch
        {
            "cn" or "zh" or "zh-cn" => "zh",
            "kr" or "ko" => "ko",
            "en" => "en",
            _ => "vi"
        };

        private static bool TryGetPoiIdFromQr(string? qrContent, out int poiId)
        {
            poiId = 0;
            if (string.IsNullOrWhiteSpace(qrContent)) return false;

            var value = qrContent.Trim();

            if (value.StartsWith("VK:", StringComparison.OrdinalIgnoreCase))
                return int.TryParse(value[3..], out poiId);

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                return false;

            if (string.Equals(uri.Scheme, "doan", StringComparison.OrdinalIgnoreCase)
                && string.Equals(uri.Host, "restaurant", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(uri.AbsolutePath.Trim('/'), out poiId);
            }

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            return segments.Length >= 2
                && string.Equals(segments[0], "q", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(segments[1], out poiId);
        }
    }
}
