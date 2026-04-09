using Microsoft.AspNetCore.Mvc;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly SyncService _syncService;

    public SyncController(SyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new
        {
            version = "1.0",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }

    [HttpGet("pois")]
    public async Task<IActionResult> GetPois()
    {
        var data = await _syncService.GetSyncDataAsync();
        return Ok(data);
    }

    [HttpPost("log")]
    public async Task<IActionResult> LogListen([FromBody] LogListenRequest req)
    {
        if (req.PoiId <= 0) return BadRequest("PoiId không hợp lệ.");
        await _syncService.LogListenAsync(
            req.PoiId, req.Language, req.AudioSource, req.TriggerType);
        return Ok(new { message = "Ghi log thành công." });
    }
}

public record LogListenRequest(
    int PoiId,
    string Language,
    string AudioSource,
    string TriggerType
);