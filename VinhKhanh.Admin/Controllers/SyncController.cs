using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly RestaurantService _svc;

    public SyncController(AppDbContext db, RestaurantService svc)
    {
        _db = db;
        _svc = svc;
    }

    // Mobile gọi để lấy toàn bộ POI đang hoạt động
    [HttpGet("pois")]
    public async Task<IActionResult> GetPois()
    {
        var pois = await _svc.GetActiveForSyncAsync();
        return Ok(new
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            pois = pois.Select(r => new
            {
                r.Id,
                r.Name,
                r.Image,
                r.Description_vi,
                r.Description_en,
                r.Description_cn,
                r.Address,
                r.Phone,
                r.OpenTime,
                r.Latitude,
                r.Longitude,
                r.RadiusMeters,
                r.Priority,
                r.AudioContent_vi,
                r.AudioContent_en,
                r.AudioContent_cn,
            })
        });
    }

    // Mobile gửi log lượt nghe (ẩn danh)
    [HttpPost("log")]
    public async Task<IActionResult> LogListen([FromBody] LogRequest req)
    {
        if (req.RestaurantId <= 0) return BadRequest();

        _db.ListenLogs.Add(new ListenLog
        {
            RestaurantId = req.RestaurantId,
            Language = req.Language ?? "vi",
            AudioSource = req.AudioSource ?? "tts",
            TriggerType = req.TriggerType ?? "gps",
            ListenedAt = DateTime.Now,
        });
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Preview TTS text cho Web Admin
    [HttpGet("/api/tts/preview")]
    public async Task<IActionResult> PreviewTts(int id, string lang = "vi")
    {
        var r = await _db.Restaurants.FindAsync(id);
        if (r is null) return NotFound();

        var text = lang switch
        {
            "en" => r.AudioContent_en,
            "cn" => r.AudioContent_cn,
            _ => r.AudioContent_vi,
        };
        return Ok(new { text });
    }
}

public record LogRequest(
    int RestaurantId,
    string? Language,
    string? AudioSource,
    string? TriggerType
);