namespace VinhKhanh.Admin.Models;

public class AudioFile
{
    public int Id { get; set; }
    public int PoiId { get; set; }       // khóa ngoại → Poi
    public string Language { get; set; } = "vi"; // "vi", "en", "zh"...
    public string FilePath { get; set; } = string.Empty; // wwwroot/audio/...
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsPublished { get; set; } = false;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Poi Poi { get; set; } = null!;
}