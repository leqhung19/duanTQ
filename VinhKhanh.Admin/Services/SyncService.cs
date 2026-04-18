using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace VinhKhanh.Admin.Services;

// DTO trả về cho mobile app
public record SyncPoiDto(
    int Id,
    string Name,
    string DescriptionVi,
    string DescriptionEn,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    int Priority,
    string? ImagePath,
    List<SyncAudioDto> AudioFiles,
    List<SyncTranslationDto> Translations
);

public record SyncAudioDto(
    int Id,
    string Language,
    string FilePath,
    bool IsPublished
);

public record SyncTranslationDto(
    string Language,
    string Content
);

public record SyncResponseDto(
    long Timestamp,   // Unix timestamp để client biết lần sync gần nhất
    List<SyncPoiDto> Pois
);

public class SyncService
{
    private readonly AppDbContext _db;

    public SyncService(AppDbContext db) => _db = db;

    // Trả toàn bộ dữ liệu cho mobile đồng bộ
    public async Task<SyncResponseDto> GetSyncDataAsync()
    {
        var pois = await _db.Pois
            .Include(p => p.AudioFiles.Where(a => a.IsPublished))
            .Include(p => p.Translations)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Priority)
            .ToListAsync();

        var dtos = pois.Select(p => new SyncPoiDto(
            p.Id,
            p.Name,
            p.DescriptionVi,
            p.DescriptionEn,
            p.Latitude,
            p.Longitude,
            p.RadiusMeters,
            p.Priority,
            p.ImagePath,
            p.AudioFiles.Select(a => new SyncAudioDto(
                a.Id, a.Language, a.FilePath, a.IsPublished
            )).ToList(),
            p.Translations.Select(t => new SyncTranslationDto(
                t.Language, t.Content
            )).ToList()
        )).ToList();

        return new SyncResponseDto(
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            dtos
        );
    }

    // Ghi log lượt nghe từ mobile (ẩn danh)
    public async Task LogListenAsync(int poiId, string language,
        string audioSource, string triggerType)
    {
        _db.ListenLogs.Add(new ListenLog
        {
            PoiId = poiId,
            Language = language,
            AudioSource = audioSource,
            TriggerType = triggerType,
            ListenedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }
}