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

            // Seed dá»¯ liá»‡u náº¿u chÆ°a cÃ³
            await SeedDataAsync();
        }

        // ============ SEED DATA ============
        private async Task SeedDataAsync()
        {
            if (_db == null) return;
            var count = await _db.Table<LocalRestaurant>().CountAsync();
            if (count > 0) return; // ÄÃ£ cÃ³ data
            var restaurants = new List<LocalRestaurant>
            {
                new LocalRestaurant
                {
                    Id = 1,
                    Name = "Ốc Oanh",
                    Image = "oc_oanh.jpg",
                    Description_vi = "Ốc Oanh là quán ốc lâu năm nổi tiếng tại con hẻm Vĩnh Khánh, Quận 4. Quán chuyên các món ốc tươi sống như ốc len xào dừa, ốc hương hấp sả, ốc bươu nhồi thịt và nghêu hấp xả. Nước chấm pha chế đặc biệt theo công thức gia truyền, vừa chua vừa cay rất đậm đà. Quán thường đông khách vào buổi tối, không khí vui vẻ và bình dân.",
                    Description_en = "Oc Oanh is a long-standing seafood snail restaurant located in the famous Vinh Khanh alley, District 4. Specializing in fresh snails such as coconut-stir-fried snails, lemongrass-steamed snails, and stuffed snails. The special dipping sauce made from a secret family recipe is perfectly sour and spicy. Always packed with customers in the evening.",
                    Description_kr = "옥 오아인은 4군 빈칸 골목에 위치한 오랜 역사의 달팽이 요리 식당입니다. 코코넛 볶음 달팽이, 레몬그라스 찐 달팽이 등 신선한 달팽이 요리를 전문으로 합니다. 비밀 가족 레시피로 만든 특별한 소스가 일품이며, 저녁 시간대에 항상 많은 손님이 찾는 곳입니다.",
                    Description_cn = "奥安是位于第四郡永庆小巷的一家历史悠久的蜗牛料理餐厅。专营新鲜蜗牛料理，如椰汁炒蜗牛、香茅蒸蜗牛等。采用秘传家族配方制作的特制蘸酱酸辣适中，每到傍晚总是顾客盈门。",
                    Address = "123 Vĩnh Khánh, Q4, TP.HCM",
                    Phone = "0901234567",
                    OpenTime = "16:00 - 23:00",
                    PriceRange = "50.000 - 150.000 VND",
                    Latitude = 10.7600,
                    Longitude = 106.7020,
                    RadiusMeters = 30,
                    Priority = 1,
                    AudioContent_vi = "Chào mừng bạn đến Ốc Oanh! Quán ốc tươi ngon nổi tiếng nhất Vĩnh Khánh. Đừng bỏ lỡ món ốc len xào dừa đặc biệt của chúng tôi!",
                    AudioContent_en = "Welcome to Oc Oanh! The most famous fresh snail restaurant in Vinh Khanh. Do not miss our special coconut stir-fried snails!",
                    AudioContent_kr = "옥 오아인에 오신 것을 환영합니다! 빈칸에서 가장 유명한 신선한 달팽이 요리 식당입니다. 코코넛 볶음 달팽이를 꼭 드셔보세요!",
                    AudioContent_cn = "欢迎来到奥安！永庆最著名的新鲜蜗牛餐厅。不要错过我们特色的椰汁炒蜗牛！"
                },
                new LocalRestaurant
                {
                    Id = 2,
                    Name = "Cơm Tấm Bà Ba",
                    Image = "com_tam.jpg",
                    Description_vi = "Cơm Tấm Bà Ba là quán cơm tấm gia đình đã hoạt động hơn 20 năm tại Vĩnh Khánh. Nổi bật với phần sườn nướng than hoa thơm lừng, bì giòn và chả trứng béo ngậy. Cơm tấm được nấu từ gạo tấm chọn lọc, hạt cơm mềm dẻo vừa phải. Nước mắm pha theo công thức riêng, ngọt thanh và cân bằng hương vị hoàn hảo. Thích hợp cho bữa sáng và bữa trưa.",
                    Description_en = "Com Tam Ba Ba is a family-run broken rice restaurant that has been operating for over 20 years in Vinh Khanh. Famous for its charcoal-grilled pork ribs, crispy shredded pork skin, and savory egg meatloaf. The rice is cooked from carefully selected broken rice grains. The house fish sauce recipe is perfectly balanced sweet and savory. Best for breakfast and lunch.",
                    Description_kr = "껌 떰 바 바는 빈칸에서 20년 이상 운영해온 가족 식당입니다. 숯불에 구운 돼지 갈비, 바삭한 돼지 껍데기, 고소한 계란 미트로프로 유명합니다. 정성껏 고른 분쌀로 지은 밥과 특제 느억맘 소스가 완벽한 조화를 이룹니다. 아침과 점심 식사로 최적입니다.",
                    Description_cn = "碎米饭芭芭是一家在永庆经营超过20年的家庭餐厅。以炭火烤猪排、香脆猪皮丝和鲜美鸡蛋肉糕著称。米饭采用精选碎米蒸制，软糯适中。自制鱼露配方酸甜平衡，非常适合早餐和午餐。",
                    Address = "456 Vĩnh Khánh, Q4, TP.HCM",
                    Phone = "0907654321",
                    OpenTime = "06:00 - 14:00",
                    PriceRange = "30.000 - 60.000 VND",
                    Latitude = 10.7570,
                    Longitude = 106.7000,
                    RadiusMeters = 30,
                    Priority = 2,
                    AudioContent_vi = "Chào bạn! Cơm Tấm Bà Ba đây. Sườn nướng than hoa thơm phức đang chờ bạn. Mở cửa từ sáu giờ sáng, ghé ăn sáng ngay nhé!",
                    AudioContent_en = "Hello! This is Com Tam Ba Ba. Our charcoal-grilled pork ribs are waiting for you. Open from six in the morning, come have breakfast now!",
                    AudioContent_kr = "안녕하세요! 껌 떰 바 바입니다. 숯불 돼지 갈비가 여러분을 기다리고 있습니다. 아침 6시부터 영업합니다!",
                    AudioContent_cn = "您好！这里是碎米饭芭芭。香喷喷的炭烤猪排正等着您。早上六点开始营业，快来吃早饭吧！"
                },
                new LocalRestaurant
                {
                    Id = 3,
                    Name = "Ốc Bà Tư",
                    Image = "oc_ba_tu.jpg",
                    Description_vi = "Ốc Bà Tư là điểm đến quen thuộc của người dân Vĩnh Khánh với hơn 15 năm kinh nghiệm. Quán phục vụ đa dạng các loại ốc như ốc mỡ luộc, ốc đá xào me, sò huyết nướng mỡ hành và mực nướng muối ớt. Giá cả bình dân, phù hợp với mọi đối tượng khách hàng. Bàn ghế đơn giản nhưng đồ ăn ngon, nước chấm vừa miệng và luôn đông vui.",
                    Description_en = "Oc Ba Tu is a familiar destination for Vinh Khanh locals with over 15 years of experience. Serving a variety of snails including boiled snails, tamarind stir-fried rock snails, grilled blood cockles with spring onion oil, and salt-chili grilled squid. Affordable prices suitable for everyone. Simple setting but delicious food and always lively.",
                    Description_kr = "옥 바 뜨는 15년 이상의 경험을 가진 빈칸 주민들의 단골 맛집입니다. 삶은 달팽이, 타마린드 볶음 달팽이, 파기름 구이 피조개, 소금고추 구이 오징어 등 다양한 해산물을 제공합니다. 저렴한 가격으로 모든 분들이 부담 없이 즐길 수 있습니다.",
                    Description_cn = "奥芭思是永庆居民熟悉的老字号，拥有超过15年的经验。供应多种蜗牛料理，包括水煮蜗牛、罗望子炒石螺、葱油烤血蚶和盐辣椒烤鱿鱼。价格实惠，适合各类顾客。环境简朴但食物美味，总是热闹非凡。",
                    Address = "78 Vĩnh Khánh, Q4, TP.HCM",
                    Phone = "0912345678",
                    OpenTime = "15:00 - 22:00",
                    PriceRange = "30.000 - 80.000 VND",
                    Latitude = 10.7612,
                    Longitude = 106.7035,
                    RadiusMeters = 30,
                    Priority = 3,
                    AudioContent_vi = "Bạn đang đến gần Ốc Bà Tư! Quán ốc bình dân ngon nổi tiếng Vĩnh Khánh. Giá rẻ, đồ ngon, ghé thử ngay đi bạn ơi!",
                    AudioContent_en = "You are near Oc Ba Tu! The most popular affordable snail restaurant in Vinh Khanh. Great food, great price, come try it now!",
                    AudioContent_kr = "옥 바 뜨 근처에 오셨습니다! 빈칸에서 가장 유명한 저렴한 달팽이 요리 식당입니다. 맛있는 음식, 저렴한 가격으로 지금 바로 방문해보세요!",
                    AudioContent_cn = "您正在靠近奥芭思！永庆最受欢迎的平价蜗牛餐厅。食物美味，价格实惠，快来试试吧！"
                },
                new LocalRestaurant
                {
                    Id = 4,
                    Name = "Cơm Tấm Sài Gòn",
                    Image = "com_tam_sg.jpg",
                    Description_vi = "Cơm Tấm Sài Gòn mang đến trải nghiệm cơm tấm đúng chuẩn Sài Gòn xưa với phần ăn đầy đặn và hương vị truyền thống. Đặc biệt có phần cơm tấm đặc biệt gồm sườn nướng, bì, chả, trứng ốp la và đồ chua. Nước mắm pha đậm vị ngọt mặn hài hòa. Quán mở từ sáng sớm phục vụ khách ăn sáng và ăn trưa với không gian sạch sẽ, thoáng mát.",
                    Description_en = "Com Tam Sai Gon brings an authentic old Saigon broken rice experience with generous portions and traditional flavors. The special combo includes grilled pork ribs, shredded pork skin, steamed meatloaf, fried egg, and pickled vegetables. The fish sauce is perfectly balanced between sweet and salty. Open from early morning for breakfast and lunch in a clean and airy setting.",
                    Description_kr = "껌 떰 사이공은 넉넉한 양과 전통적인 맛으로 정통 사이공 분쌀 경험을 선사합니다. 특별 세트는 구운 돼지 갈비, 돼지 껍데기 채, 찐 미트로프, 달걀 후라이, 절임 채소로 구성됩니다. 소스는 달콤하고 짭짤한 맛이 완벽하게 조화를 이룹니다. 이른 아침부터 점심까지 운영합니다.",
                    Description_cn = "西贡碎米饭带来正宗老西贡风味的碎米饭体验，份量十足，传统风味。特别套餐包含烤猪排、猪皮丝、蒸肉糕、煎蛋和泡菜。鱼露甜咸平衡恰到好处。从清晨开始营业，提供早餐和午餐，环境整洁通风。",
                    Address = "210 Vĩnh Khánh, Q4, TP.HCM",
                    Phone = "0923456789",
                    OpenTime = "05:30 - 13:00",
                    PriceRange = "35.000 - 65.000 VND",
                    Latitude = 10.7558,
                    Longitude = 106.6988,
                    RadiusMeters = 30,
                    Priority = 4,
                    AudioContent_vi = "Cơm Tấm Sài Gòn chào đón bạn! Phần cơm tấm đặc biệt với sườn nướng thơm lừng đang chờ. Mở từ năm rưỡi sáng, ăn sáng ngon lành nhé!",
                    AudioContent_en = "Com Tam Sai Gon welcomes you! A special broken rice combo with fragrant grilled ribs is waiting. Open from five thirty in the morning for a delicious breakfast!",
                    AudioContent_kr = "껌 떰 사이공에 오신 것을 환영합니다! 향긋한 구운 갈비와 함께하는 특별 분쌀 세트가 기다리고 있습니다. 새벽 5시 30분부터 영업합니다!",
                    AudioContent_cn = "西贡碎米饭欢迎您！香喷喷的烤排骨特别套餐正等着您。早上五点半开始营业，享受美味早餐吧！"
                },
                new LocalRestaurant
                {
                    Id = 5,
                    Name = "Bún Bò Huế Dì Sáu",
                    Image = "bun_bo_hue.jpg",
                    Description_vi = "Bún Bò Huế Dì Sáu là quán bún bò Huế chuẩn vị miền Trung hiếm có tại Sài Gòn. Nước dùng được ninh từ xương bò và xương heo trong nhiều giờ, kết hợp mắm ruốc Huế và sả tạo nên hương vị đặc trưng đậm đà khó quên. Tô bún đầy đặn với bắp bò, giò heo, chả Huế và huyết. Rau sống ăn kèm tươi ngon gồm bắp chuối, giá, húng quế. Đây là địa chỉ quen thuộc của những ai nhớ hương vị Huế.",
                    Description_en = "Bun Bo Hue Di Sau is one of the rare restaurants in Saigon serving authentic Central Vietnamese Hue beef noodle soup. The broth is simmered from beef and pork bones for many hours, combined with Hue shrimp paste and lemongrass to create an unforgettable rich and distinctive flavor. The bowl is generously filled with beef shank, pork knuckle, Hue sausage, and blood cake. Served with fresh banana blossom, bean sprouts, and basil.",
                    Description_kr = "분 보 후에 디 사우는 사이공에서 보기 드문 정통 중부 베트남 후에 소고기 쌀국수 식당입니다. 육수는 소뼈와 돼지뼈를 수 시간 동안 끓여 후에 새우젓과 레몬그라스를 더해 잊을 수 없는 진하고 독특한 맛을 만들어냅니다. 토핑은 소 정강이, 돼지족, 후에 소시지, 선지로 풍성하게 구성됩니다.",
                    Description_cn = "顺化牛肉粉迪六是西贡少有的正宗中部越南顺化牛肉粉餐厅。汤底由牛骨和猪骨熬制数小时，加入顺化虾酱和香茅，打造出令人难忘的浓郁独特风味。粉碗中盛满牛腱、猪蹄、顺化香肠和血糕，配以新鲜芭蕉花、豆芽和罗勒叶。",
                    Address = "95 Vinh Khanh, Q4, TP.HCM",
                    Phone = "0934567890",
                    OpenTime = "06:00 - 11:00",
                    PriceRange = "40.000 - 70.000 VND",
                    Latitude = 10.7595,
                    Longitude = 106.7008,
                    RadiusMeters = 30,
                    Priority = 5,
                    AudioContent_vi = "Chào mừng đến Bún Bò Huế Dì Sáu! Tô bún bò nước dùng đậm đà chuẩn vị Huế đang chờ bạn thưởng thức. Mở từ sáu giờ sáng, số lượng có hạn!",
                    AudioContent_en = "Welcome to Bun Bo Hue Di Sau! A rich and authentic Hue-style beef noodle soup is waiting for you. Open from six in the morning, limited quantity available!",
                    AudioContent_kr = "분 보 후에 디 사우에 오신 것을 환영합니다! 정통 후에 스타일의 진한 소고기 쌀국수가 기다리고 있습니다. 아침 6시부터 영업하며 수량이 한정되어 있습니다!",
                    AudioContent_cn = "欢迎来到顺化牛肉粉迪六！正宗顺化风味浓郁的牛肉粉正等着您品尝。早上六点开始营业，数量有限！"
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

        // TÃ¬m quÃ¡n theo QR content
        public async Task<LocalRestaurant?> GetByQRContentAsync(string qrContent)
        {
            await InitAsync();

            // TÃ¬m QR code
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

            // TÃ¬m quÃ¡n tÆ°Æ¡ng á»©ng
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

        // TÃ¬m quÃ¡n theo ID
        public async Task<LocalRestaurant?> GetByIdAsync(int id)
        {
            await InitAsync();
            return await _db!.Table<LocalRestaurant>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        // Láº¥y táº¥t cáº£ quÃ¡n
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

        // Sync tá»« API vá» SQLite
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

                // Insert hoáº·c Update
                await _db!.InsertOrReplaceAsync(local);
            }
        }

        // ThÃªm QR code má»›i
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
        // âœ… XÃ³a toÃ n bá»™ data cÅ© rá»“i sync láº¡i
        public async Task ClearAndSyncAsync(List<Restaurant> apiData)
        {
            await InitAsync();

            // XÃ³a háº¿t data cÅ©
            await _db!.DeleteAllAsync<LocalRestaurant>();
            await _db.DeleteAllAsync<LocalQRCode>();
            await _db.DeleteAllAsync<LocalAudioFile>();

            // Insert láº¡i toÃ n bá»™ tá»« API
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

