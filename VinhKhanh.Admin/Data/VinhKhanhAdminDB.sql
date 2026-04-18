-- ================================================================
-- VinhKhanhDb — Database dùng chung Web Admin + Mobile App
-- Dựa trên file SQL của bạn mobile, mở rộng thêm các bảng cần thiết
-- ================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'VinhKhanhDb')
    CREATE DATABASE VinhKhanhDb;
GO
USE VinhKhanhDb;
GO

-- ================================================================
-- BẢNG 1: Categories
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories')
CREATE TABLE Categories (
    Id      INT PRIMARY KEY IDENTITY(1,1),
    Name_vi NVARCHAR(100) NOT NULL,
    Name_en NVARCHAR(100),
    Name_kr NVARCHAR(100),
    Name_cn NVARCHAR(100),
    Icon    NVARCHAR(200)
);
GO

-- ================================================================
-- BẢNG 2: Restaurants (POI — dùng chung Mobile + Web)
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Restaurants')
CREATE TABLE Restaurants (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    CategoryId      INT FOREIGN KEY REFERENCES Categories(Id),
    Name            NVARCHAR(200) NOT NULL,
    Image           NVARCHAR(500),

    -- Mô tả ngắn hiển thị trong app/web
    Description_vi  NVARCHAR(MAX),
    Description_en  NVARCHAR(MAX),
    Description_cn  NVARCHAR(MAX),

    -- Thông tin liên hệ
    Address         NVARCHAR(300),
    Phone           NVARCHAR(20),
    OpenTime        NVARCHAR(50),
    PriceRange      NVARCHAR(50),

    -- Tọa độ GPS
    Latitude        FLOAT NOT NULL DEFAULT 0,
    Longitude       FLOAT NOT NULL DEFAULT 0,
    RadiusMeters    FLOAT DEFAULT 30,
    Priority        INT   DEFAULT 1,

    -- Nội dung TTS — văn bản đọc khi kích hoạt Geofence
    AudioContent_vi NVARCHAR(MAX),
    AudioContent_en NVARCHAR(MAX),
    AudioContent_cn NVARCHAR(MAX),

    IsActive        BIT      DEFAULT 1,
    CreatedAt       DATETIME DEFAULT GETDATE(),
    UpdatedAt       DATETIME DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Restaurants_LatLng
    ON Restaurants(Latitude, Longitude);
GO

-- ================================================================
-- BẢNG 3: QRCodes
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QRCodes')
CREATE TABLE QRCodes (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    RestaurantId INT NOT NULL FOREIGN KEY REFERENCES Restaurants(Id)
                     ON DELETE CASCADE,
    QRContent    NVARCHAR(500) NOT NULL,  -- Ví dụ: "VK:1"
    IsActive     BIT DEFAULT 1,
    CreatedAt    DATETIME DEFAULT GETDATE()
);
GO

-- ================================================================
-- BẢNG 4: ListenLogs — Analytics ẩn danh (không lưu thông tin cá nhân)
-- Dùng chung: Mobile ghi log, Web đọc thống kê
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ListenLogs')
CREATE TABLE ListenLogs (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    RestaurantId INT NOT NULL FOREIGN KEY REFERENCES Restaurants(Id)
                     ON DELETE CASCADE,
    Language     NVARCHAR(10)  DEFAULT 'vi', -- vi | en | cn
    AudioSource  NVARCHAR(10)  DEFAULT 'tts', -- tts | file
    TriggerType  NVARCHAR(10)  DEFAULT 'gps', -- gps | qr
    ListenedAt   DATETIME      DEFAULT GETDATE()
    -- KHÔNG lưu IP, IMEI, hay bất kỳ định danh cá nhân
);
GO

CREATE INDEX IX_ListenLogs_RestaurantId ON ListenLogs(RestaurantId);
CREATE INDEX IX_ListenLogs_ListenedAt   ON ListenLogs(ListenedAt);
GO

-- ================================================================
-- BẢNG 5: ActiveSessions — Người dùng đang dùng app (real-time)
-- Mobile kết nối SignalR → ghi vào đây
-- Web đọc COUNT(*) để hiện số người trực tuyến
-- ================================================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ActiveSessions')
CREATE TABLE ActiveSessions (
    Id             INT PRIMARY KEY IDENTITY(1,1),
    ConnectionId   NVARCHAR(200) NOT NULL UNIQUE, -- SignalR Connection ID
    DevicePlatform NVARCHAR(20),                  -- android | ios
    Language       NVARCHAR(10) DEFAULT 'vi',     -- ngôn ngữ đang dùng
    ConnectedAt    DATETIME DEFAULT GETDATE(),
    LastPing       DATETIME DEFAULT GETDATE()
);
GO

-- ================================================================
-- DỮ LIỆU MẪU
-- ================================================================
INSERT INTO Categories (Name_vi, Name_en, Name_cn, Icon) VALUES
(N'Ốc & Hải sản', 'Seafood',    N'海鲜',  'seafood.png'),
(N'Cơm Tấm',      'Broken Rice', N'碎米饭', 'rice.png'),
(N'Bún & Phở',    'Noodle',      N'面条',  'noodle.png');
GO

INSERT INTO Restaurants
(CategoryId, Name, Image,
 Description_vi, Description_en, Description_cn,
 Address, Phone, OpenTime, PriceRange,
 Latitude, Longitude, RadiusMeters, Priority,
 AudioContent_vi, AudioContent_en, AudioContent_cn)
VALUES
(1, N'Ốc Oanh', 'oc_oanh.jpg',
 N'Quán ốc nổi tiếng tại Vĩnh Khánh, chuyên ốc hấp sả và hải sản tươi sống.',
 N'Famous seafood restaurant on Vinh Khanh street, specializing in fresh snails.',
 N'永庆街著名的海鲜餐厅，专营新鲜蜗牛和海鲜。',
 N'123 Vĩnh Khánh, Quận 4, TP.HCM', '0901234567', '16:00 - 23:00', N'50k - 150k',
 10.7600, 106.7020, 30, 1,
 N'Bạn đang đến gần Ốc Oanh — quán ốc nổi tiếng nhất phố Vĩnh Khánh.',
 N'You are approaching Oc Oanh, the most famous seafood spot on Vinh Khanh.',
 N'您正在接近玉安，永庆街最著名的海鲜小吃店。'),

(2, N'Cơm Tấm Bà Ba', 'com_tam.jpg',
 N'Cơm tấm sườn nướng thơm ngon, hoạt động từ 1990. Nổi tiếng với nước mắm pha đặc trưng.',
 N'Grilled pork broken rice since 1990, famous for its special fish sauce.',
 N'1990年开业的烤猪肉碎米饭店，以特制鱼露著称。',
 N'456 Vĩnh Khánh, Quận 4, TP.HCM', '0907654321', '06:00 - 14:00', N'30k - 60k',
 10.7570, 106.7000, 30, 2,
 N'Bạn đang đến gần Cơm Tấm Bà Ba — quán cơm tấm truyền thống hơn 30 năm.',
 N'You are near Com Tam Ba Ba, a traditional broken rice restaurant for 30 years.',
 N'您正在接近巴巴碎米饭，一家经营30年以上的传统餐厅。');
GO

INSERT INTO QRCodes (RestaurantId, QRContent) VALUES (1, 'VK:1'), (2, 'VK:2');
GO