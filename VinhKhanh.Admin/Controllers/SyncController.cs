using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _db;

    public SyncController(AppDbContext db) => _db = db;

    // Mobile goi de lay toan bo POI dang hoat dong.
    [HttpGet("pois")]
    public async Task<IActionResult> GetPois()
    {
        var restaurants = await _db.Restaurants
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.QRCodes)
            .Include(r => r.AudioFiles)
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();

        var pois = restaurants
            .Select(r => new SyncPoiResponse(
                r.Id,
                r.CategoryId,
                r.Category == null ? null : r.Category.Name_vi,
                r.Name,
                ToImageUrl(r.Image),
                new LocalizedText(r.Description_vi, r.Description_en, r.Description_kr, r.Description_cn),
                r.Address,
                r.Phone,
                r.OpenTime,
                r.PriceRange,
                r.Latitude,
                r.Longitude,
                r.RadiusMeters,
                r.Priority,
                new LocalizedText(r.AudioContent_vi, r.AudioContent_en, r.AudioContent_kr, r.AudioContent_cn),
                r.AudioFiles
                    .Where(a => a.IsPublished)
                    .OrderBy(a => a.Language)
                    .Select(a => new SyncAudioFileResponse(
                        a.Id,
                        a.Language,
                        ToPublicUrl(a.FilePath),
                        a.FileSizeBytes))
                    .ToList(),
                r.QRCodes
                    .Where(q => q.IsActive)
                    .OrderBy(q => q.Id)
                    .Select(q => q.QRContent)
                    .ToList(),
                r.UpdatedAt))
            .ToList();

        return Ok(new SyncPoisResponse(
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            pois.Count,
            pois));
    }

    // Mobile gui log luot nghe an danh.
    [HttpPost("log")]
    public async Task<IActionResult> LogListen([FromBody] LogRequest req)
    {
        if (req.RestaurantId <= 0)
            return BadRequest(new { message = "restaurantId khong hop le." });

        var exists = await _db.Restaurants.AnyAsync(r => r.Id == req.RestaurantId);
        if (!exists)
            return NotFound(new { message = "Khong tim thay POI." });

        _db.ListenLogs.Add(new ListenLog
        {
            RestaurantId = req.RestaurantId,
            Language = Normalize(req.Language, "vi"),
            AudioSource = Normalize(req.AudioSource, "tts"),
            TriggerType = Normalize(req.TriggerType, "gps"),
            ListenedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "logged" });
    }

    // Mobile ping an danh de Web Admin dem so du khach dang online.
    [HttpPost("session/ping")]
    public async Task<IActionResult> PingSession([FromBody] SessionPingRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SessionId))
            return BadRequest(new { message = "sessionId khong hop le." });

        var sessionId = req.SessionId.Trim();
        var now = DateTime.Now;
        var session = await _db.ActiveSessions
            .FirstOrDefaultAsync(s => s.ConnectionId == sessionId);

        if (session is null)
        {
            _db.ActiveSessions.Add(new ActiveSession
            {
                ConnectionId = sessionId,
                DevicePlatform = Normalize(req.Platform, "unknown"),
                Language = Normalize(req.Language, "vi"),
                ConnectedAt = now,
                LastPing = now
            });
        }
        else
        {
            session.DevicePlatform = Normalize(req.Platform, session.DevicePlatform ?? "unknown");
            session.Language = Normalize(req.Language, session.Language);
            session.LastPing = now;
        }

        await _db.SaveChangesAsync();
        var activeDeadline = now.AddMinutes(-3);
        var activeUsers = await _db.ActiveSessions.CountAsync(s => s.LastPing >= activeDeadline);
        return Ok(new { activeUsers });
    }

    // Kiem tra nhanh API dang ket noi database nao.
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var canConnect = await _db.Database.CanConnectAsync();
        var database = canConnect
            ? await _db.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS [Value]").SingleAsync()
            : null;
        var activePois = canConnect
            ? await _db.Restaurants.CountAsync(r => r.IsActive)
            : 0;

        return Ok(new
        {
            canConnect,
            database,
            activePois,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }

    // Preview TTS text cho Web Admin.
    [HttpGet("/api/tts/preview")]
    public async Task<IActionResult> PreviewTts(int id, string lang = "vi")
    {
        var r = await _db.Restaurants.FindAsync(id);
        if (r is null) return NotFound();

        var text = lang switch
        {
            "en" => r.AudioContent_en,
            "ko" or "kr" => r.AudioContent_kr,
            "cn" => r.AudioContent_cn,
            "zh" => r.AudioContent_cn,
            _ => r.AudioContent_vi,
        };
        return Ok(new { text });
    }

    private static string Normalize(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToLowerInvariant();

    private string? ToImageUrl(string? image)
    {
        if (string.IsNullOrWhiteSpace(image)) return null;
        if (Uri.TryCreate(image, UriKind.Absolute, out _)) return image;

        var path = image.Replace('\\', '/').TrimStart('/');
        if (!path.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            path = $"images/{path}";

        return ToPublicUrl(path);
    }

    private string? ToPublicUrl(string? pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl)) return null;
        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out _)) return pathOrUrl;

        var path = pathOrUrl.Replace('\\', '/').TrimStart('/');
        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}/{path}";
    }
}

public record SyncPoisResponse(
    long Timestamp,
    int Count,
    List<SyncPoiResponse> Pois);

public record SyncPoiResponse(
    int Id,
    int? CategoryId,
    string? CategoryName,
    string Name,
    string? ImageUrl,
    LocalizedText Description,
    string? Address,
    string? Phone,
    string? OpenTime,
    string? PriceRange,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    int Priority,
    LocalizedText AudioText,
    List<SyncAudioFileResponse> AudioFiles,
    List<string> QrCodes,
    DateTime UpdatedAt);

public record SyncAudioFileResponse(
    int Id,
    string Language,
    string? Url,
    long FileSizeBytes);

public record LocalizedText(
    string? Vi,
    string? En,
    string? Ko,
    string? Cn);

public record LogRequest(
    int RestaurantId,
    string? Language,
    string? AudioSource,
    string? TriggerType);

public record SessionPingRequest(
    string SessionId,
    string? Platform,
    string? Language);
