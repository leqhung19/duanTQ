using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Services;

public class AudioService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AudioService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Upload file audio mới cho 1 POI
    public async Task<AudioFile?> UploadAsync(
        int poiId, string language, IFormFile file)
    {
        // Validate
        if (file.Length == 0 || file.Length > 20 * 1024 * 1024) // giới hạn 20MB
            return null;

        var allowed = new[] { ".mp3", ".wav", ".m4a" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext)) return null;

        // Tạo thư mục lưu file
        var audioDir = Path.Combine(_env.WebRootPath, "audio", poiId.ToString());
        Directory.CreateDirectory(audioDir);

        // Tên file: poi_{id}_{lang}_{timestamp}.mp3
        var fileName = $"poi_{poiId}_{language}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
        var filePath = Path.Combine(audioDir, fileName);
        var urlPath = $"/audio/{poiId}/{fileName}";

        // Lưu file vật lý
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Xóa file cũ cùng ngôn ngữ (nếu có) — thay thế, không giữ lại
        var oldAudio = await _db.AudioFiles
            .FirstOrDefaultAsync(a => a.PoiId == poiId && a.Language == language);

        if (oldAudio is not null)
        {
            var oldPath = Path.Combine(_env.WebRootPath, oldAudio.FilePath.TrimStart('/'));
            if (File.Exists(oldPath)) File.Delete(oldPath);
            _db.AudioFiles.Remove(oldAudio);
        }

        // Lưu vào database
        var audioFile = new AudioFile
        {
            PoiId = poiId,
            Language = language,
            FilePath = urlPath,
            FileName = file.FileName,
            FileSizeBytes = file.Length,
            IsPublished = false, // chưa publish ngay — cần admin xét duyệt
            UploadedAt = DateTime.UtcNow,
        };

        _db.AudioFiles.Add(audioFile);
        await _db.SaveChangesAsync();
        return audioFile;
    }

    // Publish / Unpublish audio (Admin duyệt)
    public async Task<bool> SetPublishedAsync(int audioId, bool isPublished)
    {
        var audio = await _db.AudioFiles.FindAsync(audioId);
        if (audio is null) return false;

        audio.IsPublished = isPublished;
        await _db.SaveChangesAsync();
        return true;
    }

    // Xóa audio
    public async Task<bool> DeleteAsync(int audioId)
    {
        var audio = await _db.AudioFiles.FindAsync(audioId);
        if (audio is null) return false;

        var filePath = Path.Combine(_env.WebRootPath, audio.FilePath.TrimStart('/'));
        if (File.Exists(filePath)) File.Delete(filePath);

        _db.AudioFiles.Remove(audio);
        await _db.SaveChangesAsync();
        return true;
    }

    // Lấy danh sách audio của 1 POI
    public async Task<List<AudioFile>> GetByPoiAsync(int poiId)
    {
        return await _db.AudioFiles
            .Where(a => a.PoiId == poiId)
            .OrderBy(a => a.Language)
            .ToListAsync();
    }
}