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
 N'您正在接近碎米饭餐厅');
GO

INSERT INTO QRCodes (RestaurantId, QRContent) VALUES
(1, 'doan://restaurant/1'),
(2, 'doan://restaurant/2');
GO