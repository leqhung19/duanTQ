namespace VinhKhanh.Admin.Models;

public class Translation
{
    public int Id { get; set; }
    public int PoiId { get; set; }
    public string Language { get; set; } = string.Empty; // "en", "zh", "ko"...
    public string Content { get; set; } = string.Empty; // văn bản thuyết minh
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Poi Poi { get; set; } = null!;
}