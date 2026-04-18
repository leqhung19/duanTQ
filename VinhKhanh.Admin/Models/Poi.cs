using System.ComponentModel.DataAnnotations;

namespace VinhKhanh.Admin.Models;

public class Poi
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string DescriptionVi { get; set; } = string.Empty; // Tiếng Việt

    [MaxLength(2000)]
    public string DescriptionEn { get; set; } = string.Empty; // Tiếng Anh

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; } = 30.0;
    public int Priority { get; set; } = 0;   // ưu tiên cao hơn = số lớn hơn
    public bool IsActive { get; set; } = true;

    public string? ImagePath { get; set; }         // đường dẫn ảnh minh hoạ
    public string? OwnerId { get; set; }         // khóa ngoại → AspNetUsers

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<AudioFile> AudioFiles { get; set; } = [];
    public ICollection<Translation> Translations { get; set; } = [];
    public ICollection<ListenLog> ListenLogs { get; set; } = [];
}