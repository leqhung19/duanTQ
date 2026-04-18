// Models/Restaurant.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinhKhanh.Admin.Models;

public class Restaurant
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên điểm")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }

    // Mô tả ngắn
    public string? Description_vi { get; set; }
    public string? Description_en { get; set; }
    public string? Description_cn { get; set; }

    // Liên hệ
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? OpenTime { get; set; }
    public string? PriceRange { get; set; }

    // GPS
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; } = 30;
    public int Priority { get; set; } = 1;

    // Nội dung TTS đọc khi Geofence kích hoạt
    public string? AudioContent_vi { get; set; }
    public string? AudioContent_en { get; set; }
    public string? AudioContent_cn { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Category? Category { get; set; }
    public ICollection<ListenLog> ListenLogs { get; set; } = [];
    public ICollection<QRCode> QRCodes { get; set; } = [];
}

public class Category
{
    public int Id { get; set; }
    public string Name_vi { get; set; } = string.Empty;
    public string? Name_en { get; set; }
    public string? Name_cn { get; set; }
    public string? Icon { get; set; }
    public ICollection<Restaurant> Restaurants { get; set; } = [];
}

public class QRCode
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string QRContent { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Restaurant? Restaurant { get; set; }
}

public class ListenLog
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Language { get; set; } = "vi";   // vi | en | cn
    public string AudioSource { get; set; } = "tts";  // tts | file
    public string TriggerType { get; set; } = "gps";  // gps | qr
    public DateTime ListenedAt { get; set; } = DateTime.Now;
    public Restaurant? Restaurant { get; set; }
}

public class ActiveSession
{
    public int Id { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string? DevicePlatform { get; set; }  // android | ios
    public string Language { get; set; } = "vi";
    public DateTime ConnectedAt { get; set; } = DateTime.Now;
    public DateTime LastPing { get; set; } = DateTime.Now;
}