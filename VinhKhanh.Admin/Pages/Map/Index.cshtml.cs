using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;

namespace VinhKhanh.Admin.Pages.Map;

[Authorize]
public class IndexModel : PageModel
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AppDbContext _db;

    public int ActivePois { get; set; }
    public int TotalListens { get; set; }
    public int QrListens { get; set; }
    public int TotalQrScans { get; set; }
    public int TotalInteractions { get; set; }
    public int AnonymousQrVisitors { get; set; }
    public int HotPoiCount { get; set; }
    public double CenterLat { get; set; } = 10.7590;
    public double CenterLng { get; set; } = 106.7010;
    public string PoiJson { get; set; } = "[]";
    public string HeatJson { get; set; } = "[]";
    public List<TopPoiItem> TopPois { get; set; } = [];

    public IndexModel(AppDbContext db) => _db = db;

    public async Task OnGetAsync()
    {
        var stats = await _db.ListenLogs
            .AsNoTracking()
            .GroupBy(l => l.RestaurantId)
            .Select(g => new PoiListenStats(
                g.Key,
                g.Count(),
                g.Count(l => l.TriggerType == "qr"),
                g.Count(l => l.TriggerType == "gps")))
            .ToDictionaryAsync(x => x.RestaurantId);

        var qrScanStats = await _db.QrScanLogs
            .AsNoTracking()
            .GroupBy(l => l.RestaurantId)
            .Select(g => new PoiQrScanStats(
                g.Key,
                g.Count(),
                g.Where(l => l.AnonymousSessionId != null && l.AnonymousSessionId != "")
                    .Select(l => l.AnonymousSessionId!)
                    .Distinct()
                    .Count()))
            .ToDictionaryAsync(x => x.RestaurantId);

        var pois = await _db.Restaurants
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Address,
                r.Latitude,
                r.Longitude,
                r.RadiusMeters,
                r.IsActive
            })
            .ToListAsync();

        ActivePois = pois.Count;
        TotalListens = stats.Values.Sum(s => s.TotalListens);
        QrListens = stats.Values.Sum(s => s.QrListens);
        TotalQrScans = qrScanStats.Values.Sum(s => s.TotalQrScans);
        TotalInteractions = TotalListens + TotalQrScans;
        AnonymousQrVisitors = await _db.QrScanLogs
            .AsNoTracking()
            .Where(l => l.AnonymousSessionId != null && l.AnonymousSessionId != "")
            .Select(l => l.AnonymousSessionId!)
            .Distinct()
            .CountAsync();

        if (pois.Count > 0)
        {
            CenterLat = pois.Average(p => p.Latitude);
            CenterLng = pois.Average(p => p.Longitude);
        }

        var mapPois = pois.Select(p =>
        {
            stats.TryGetValue(p.Id, out var listenStats);
            qrScanStats.TryGetValue(p.Id, out var scanStats);
            var totalListens = listenStats?.TotalListens ?? 0;
            var totalQrScans = scanStats?.TotalQrScans ?? 0;
            var interestScore = totalListens + totalQrScans;

            return new MapPoiItem(
                p.Id,
                p.Name,
                p.Address,
                p.Latitude,
                p.Longitude,
                p.RadiusMeters,
                p.IsActive,
                totalListens,
                listenStats?.QrListens ?? 0,
                listenStats?.GpsListens ?? 0,
                totalQrScans,
                scanStats?.AnonymousVisitors ?? 0,
                interestScore);
        }).ToList();

        HotPoiCount = mapPois.Count(p => p.InterestScore > 0);

        TopPois = mapPois
            .Where(p => p.InterestScore > 0)
            .OrderByDescending(p => p.InterestScore)
            .Take(5)
            .Select(p => new TopPoiItem(
                p.Name,
                p.Address,
                p.InterestScore,
                p.TotalListens,
                p.QrListens,
                p.GpsListens,
                p.QrScans,
                p.AnonymousQrVisitors))
            .ToList();

        var heatPoints = mapPois
            .Where(p => p.InterestScore > 0)
            .Select(p => new object[] { p.Latitude, p.Longitude, Math.Max(1, p.InterestScore) })
            .ToList();

        PoiJson = JsonSerializer.Serialize(mapPois, JsonOptions);
        HeatJson = JsonSerializer.Serialize(heatPoints, JsonOptions);
    }

    public record PoiListenStats(int RestaurantId, int TotalListens, int QrListens, int GpsListens);
    public record PoiQrScanStats(int RestaurantId, int TotalQrScans, int AnonymousVisitors);

    public record MapPoiItem(
        int Id,
        string Name,
        string? Address,
        double Latitude,
        double Longitude,
        double RadiusMeters,
        bool IsActive,
        int TotalListens,
        int QrListens,
        int GpsListens,
        int QrScans,
        int AnonymousQrVisitors,
        int InterestScore);

    public record TopPoiItem(
        string Name,
        string? Address,
        int InterestScore,
        int TotalListens,
        int QrListens,
        int GpsListens,
        int QrScans,
        int AnonymousQrVisitors);
}
