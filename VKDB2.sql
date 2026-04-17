CREATE DATABASE DB;
GO

USE DoAnDB;
GO

-- =====================
-- 1. CATEGORIES
-- =====================
CREATE TABLE Categories (
    Id       INT PRIMARY KEY IDENTITY(1,1),
    Name_vi  NVARCHAR(100) NOT NULL,
    Name_en  NVARCHAR(100),
    Name_kr  NVARCHAR(100),
    Name_cn  NVARCHAR(100),
    Icon     NVARCHAR(200)
);
GO

-- =====================
-- 2. RESTAURANTS
-- =====================
CREATE TABLE Restaurants (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    CategoryId      INT FOREIGN KEY REFERENCES Categories(Id),
    Name            NVARCHAR(200) NOT NULL,
    Image           NVARCHAR(500),
    Description_vi  NVARCHAR(MAX),
    Description_en  NVARCHAR(MAX),
    Description_kr  NVARCHAR(MAX),
    Description_cn  NVARCHAR(MAX),
    Address         NVARCHAR(300),
    Phone           NVARCHAR(20),
    OpenTime        NVARCHAR(50),
    PriceRange      NVARCHAR(50),
    Latitude        FLOAT NOT NULL,
    Longitude       FLOAT NOT NULL,
    RadiusMeters    FLOAT DEFAULT 30,
    Priority        INT DEFAULT 1,
    AudioContent_vi NVARCHAR(MAX),
    AudioContent_en NVARCHAR(MAX),
    AudioContent_kr NVARCHAR(MAX),
    AudioContent_cn NVARCHAR(MAX),
    IsActive        BIT DEFAULT 1,
    CreatedAt       DATETIME DEFAULT GETDATE()
);
GO
SELECT Id, Name FROM Restaurants;
GO
-- =====================
-- 3. QR CODES
-- =====================
CREATE TABLE QRCodes (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    RestaurantId INT FOREIGN KEY REFERENCES Restaurants(Id),
    QRContent    NVARCHAR(500) NOT NULL,
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
-- =====================
-- DỮ LIỆU MẪU
-- =====================

INSERT INTO Categories (Name_vi, Name_en, Name_kr, Name_cn, Icon) VALUES
(N'Ốc', 'Seafood', N'해산물', N'海鲜', 'seafood.png'),
(N'Cơm Tấm', 'Broken Rice', N'분쌀', N'碎米饭', 'rice.png'),
(N'Bún', 'Noodle', N'국수', N'面条', 'noodle.png');
GO

INSERT INTO Restaurants
(CategoryId, Name, Image,
 Description_vi, Description_en, Description_kr, Description_cn,
 Address, Phone, OpenTime, PriceRange,
 Latitude, Longitude, RadiusMeters, Priority,
 AudioContent_vi, AudioContent_en, AudioContent_kr, AudioContent_cn)
VALUES
(1, N'Ốc Oanh', 'oc_oanh.jpg',
 N'Quán ốc nổi tiếng tại Vĩnh Khánh',
 'Famous seafood restaurant in Vinh Khanh',
 N'빈칸의 유명한 해산물 식당',
 N'永庆著名的海鲜餐厅',
 N'123 Vĩnh Khánh, Q4, TP.HCM', '0901234567',
 '16:00 - 23:00', N'50.000 - 150.000 VND',
 10.7600, 106.7020, 30, 1,
 N'Bạn đang đến gần Ốc Oanh, quán ốc nổi tiếng tại Vĩnh Khánh',
 'You are approaching Oc Oanh, a famous seafood restaurant',
 N'유명한 해산물 식당 옥 오아인에 접근 중입니다',
 N'您正在接近永庆著名的海鲜餐厅'),

(2, N'Cơm Tấm Bà Ba', 'com_tam.jpg',
 N'Cơm tấm sườn nướng thơm ngon',
 'Grilled pork broken rice',
 N'구운 돼지고기 분쌀',
 N'烤猪肉碎米饭',
 N'456 Vĩnh Khánh, Q4, TP.HCM', '0907654321',
 '06:00 - 14:00', N'30.000 - 60.000 VND',
 10.7570, 106.7000, 30, 2,
 N'Bạn đang đến gần Cơm Tấm Bà Ba, quán cơm tấm ngon',
 'You are approaching Com Tam Ba Ba restaurant',
 N'껌 떰 바 바 식당에 접근 중입니다',
 N'您正在接近碎米饭餐厅'),

(1, N'Ốc Bà Tư', 'oc_ba_tu.jpg',
 N'Quán ốc bình dân giá rẻ, đông khách',
 'Affordable seafood restaurant, always crowded',
 N'저렴한 해산물 식당',
 N'价格实惠的海鲜餐厅',
 N'78 Vĩnh Khánh, Q4, TP.HCM',
 '0912345678', '15:00 - 22:00',
 N'30.000 - 80.000 VND',
 10.7612, 106.7035, 30, 3,
 N'Bạn đang đến gần Ốc Bà Tư, quán ốc bình dân nổi tiếng',
 'You are approaching Oc Ba Tu restaurant',
 N'당신은 옥 바 투 레스토랑에 접근하고 있습니다',
 N'你正在接近Oc Ba Tu餐厅'),

(2,N'Cơm Tấm Sài Gòn', 'com_tam_sg.jpg',
 N'Cơm tấm đặc biệt với sườn nướng và chả trứng',
 'Special broken rice with grilled pork and egg',
 N'특별한 분쌀 요리',
 N'特别碎米饭套餐',
 N'210 Vĩnh Khánh, Q4, TP.HCM',
 '0923456789', '05:30 - 13:00',
 N'35.000 - 65.000 VND',
 10.7558, 106.6988, 30, 4,
 N'Bạn đang đến gần Cơm Tấm Sài Gòn',
 'You are approaching Com Tam Sai Gon restaurant',
 N'당신은 콤탐 사이공 레스토랑에 접근하고 있습니다',
 N'您正接近西贡梳米餐厅'),

(3, N'Bún Bò Huế Dì Sáu', 'bun_bo_hue.jpg',
 N'Bún bò Huế chuẩn vị miền Trung, nước dùng đậm đà',
 'Authentic Hue beef noodle soup with rich broth',
 N'정통 후에 소고기 쌀국수',
 N'正宗顺化牛肉粉',
 N'95 Vĩnh Khánh, Q4, TP.HCM',
 '0934567890', '06:00 - 11:00',
 N'40.000 - 70.000 VND',
 10.7595, 106.7008, 30, 5,
 N'Bạn đang đến gần Bún Bò Huế Dì Sáu, đặc sản miền Trung',
 'You are approaching Bun Bo Hue Di Sau restaurant',
 N'당신은 분보후에 디사우 음식점에 접근하고 있습니다',
 N'你正在接近Bun Bo Hue Di Sau餐厅');
GO
UPDATE Restaurants SET Image = 'oc_oanh.jpg'    WHERE Id = 1;
UPDATE Restaurants SET Image = 'com_tam.jpg'    WHERE Id = 2;
UPDATE Restaurants SET Image = 'oc_ba_tu.jpg'   WHERE Id = 3;
UPDATE Restaurants SET Image = 'com_tam_sg.jpg' WHERE Id = 4;
UPDATE Restaurants SET Image = 'bun_bo_hue.jpg' WHERE Id = 5;
GO
INSERT INTO QRCodes (RestaurantId, QRContent) VALUES
(1, 'doan://restaurant/1'),
(2, 'doan://restaurant/2'),
(3, 'doan://restaurant/3'),
(4, 'doan://restaurant/4'),
(5, 'doan://restaurant/5');
GO
SELECT Id, Name, IsActive FROM Restaurants;
GO
