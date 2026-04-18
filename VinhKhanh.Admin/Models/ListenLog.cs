/*
namespace VinhKhanh.Admin.Models;

public class ListenLog
{
    public int Id { get; set; }
    public int PoiId { get; set; }
    public string Language { get; set; } = "vi";
    public string AudioSource { get; set; } = "file"; // "file" hoặc "tts"
    public string TriggerType { get; set; } = "gps";  // "gps" hoặc "qr"
    public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
    // KHÔNG lưu IP, IMEI hay bất kỳ thông tin cá nhân nào — theo NF-04 trong PRD

    // Navigation
    public Restaurant Poi { get; set; } = null!;
}
*/