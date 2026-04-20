using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Services;

public class AudioService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".mp3", ".wav", ".m4a" };

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly int _maxSizeBytes;

    public AudioService(AppDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _maxSizeBytes = config.GetValue<int?>("AppSettings:AudioMaxSizeMb").GetValueOrDefault(20) * 1024 * 1024;
    }

    public async Task<List<AudioFile>> GetByRestaurantAsync(int restaurantId) =>
        await _db.AudioFiles
            .Where(a => a.RestaurantId == restaurantId)
            .OrderBy(a => a.Language)
            .ThenByDescending(a => a.UploadedAt)
            .ToListAsync();

    public async Task<(AudioFile? Audio, string? Error)> UploadAsync(int restaurantId, string language, IFormFile file)
    {
        var normalizedLanguage = NormalizeLanguage(language);
        if (normalizedLanguage is null)
            return (null, "Ngon ngu audio khong hop le.");

        if (file.Length <= 0)
            return (null, "File audio rong.");

        if (file.Length > _maxSizeBytes)
            return (null, $"File audio vuot qua gioi han {_maxSizeBytes / 1024 / 1024}MB.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            return (null, "Chi ho tro file .mp3, .wav, .m4a.");

        var exists = await _db.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!exists)
            return (null, "Khong tim thay POI.");

        var folder = Path.Combine(_env.WebRootPath, "audio", restaurantId.ToString());
        Directory.CreateDirectory(folder);

        var fileName = $"{normalizedLanguage}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        var audio = new AudioFile
        {
            RestaurantId = restaurantId,
            Language = normalizedLanguage,
            FileName = fileName,
            FilePath = $"/audio/{restaurantId}/{fileName}",
            FileSizeBytes = file.Length,
            IsPublished = true,
            UploadedAt = DateTime.UtcNow
        };

        _db.AudioFiles.Add(audio);
        await _db.SaveChangesAsync();
        return (audio, null);
    }

    public async Task<bool> SetPublishedAsync(int audioId, bool publish)
    {
        var audio = await _db.AudioFiles.FindAsync(audioId);
        if (audio is null) return false;

        audio.IsPublished = publish;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int audioId)
    {
        var audio = await _db.AudioFiles.FindAsync(audioId);
        if (audio is null) return false;

        var physicalPath = Path.Combine(_env.WebRootPath, audio.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        _db.AudioFiles.Remove(audio);
        await _db.SaveChangesAsync();

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        return true;
    }

    public static string? NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language)) return null;

        return language.Trim().ToLowerInvariant() switch
        {
            "vi" or "vn" => "vi",
            "en" => "en",
            "ko" or "kr" => "ko",
            "cn" or "zh" or "zh-cn" => "cn",
            _ => null
        };
    }
}
