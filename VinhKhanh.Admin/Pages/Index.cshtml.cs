using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ActiveSessionService _activeSessions;

    public int TotalPois { get; set; }
    public int ActivePois { get; set; }
    public int ActiveUsers { get; set; }
    public int TodayListens { get; set; }
    public int TotalListens { get; set; }
    public int TodayQrScans { get; set; }
    public int TotalQrScans { get; set; }

    public List<TopPoiItem> TopPois { get; set; } = [];
    public List<SupportedLanguageItem> SupportedLanguages { get; set; } =
    [
        new("vi", "Tiếng Việt"),
        new("en", "English"),
        new("ko", "Korean"),
        new("cn", "Chinese")
    ];
    public List<ListenLog> RecentLogs { get; set; } = [];
    public List<QrScanLog> RecentQrScans { get; set; } = [];

    public IndexModel(AppDbContext db, ActiveSessionService activeSessions)
    {
        _db = db;
        _activeSessions = activeSessions;
    }

    public async Task OnGetAsync()
    {
        var nowVn = DateTime.UtcNow.AddHours(7);
        var todayVnStartUtc = nowVn.Date.AddHours(-7);

        TotalPois = await _db.Restaurants.CountAsync();
        ActivePois = await _db.Restaurants.CountAsync(r => r.IsActive);
        ActiveUsers = await _activeSessions.CountActiveUsersAsync();
        TodayListens = await _db.ListenLogs.CountAsync(l => l.ListenedAt >= todayVnStartUtc);
        TotalListens = await _db.ListenLogs.CountAsync();
        TodayQrScans = await _db.QrScanLogs.CountAsync(l => l.ScannedAt >= todayVnStartUtc);
        TotalQrScans = await _db.QrScanLogs.CountAsync();

        // Top 5 POI nghe nhiều nhất
        var poiNames = await _db.Restaurants
            .ToDictionaryAsync(r => r.Id, r => r.Name);

        var grouped = await _db.ListenLogs
            .GroupBy(l => l.RestaurantId)
            .Select(g => new { PoiId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();

        var max = grouped.FirstOrDefault()?.Count ?? 1;
        TopPois = grouped.Select(g => new TopPoiItem(
            poiNames.GetValueOrDefault(g.PoiId, "?"),
            g.Count,
            (int)(g.Count * 100.0 / max)
        )).ToList();

        // 10 lượt nghe gần nhất
        RecentLogs = await _db.ListenLogs
            .Include(l => l.Restaurant)
            .OrderByDescending(l => l.ListenedAt)
            .Take(10)
            .ToListAsync();

        RecentQrScans = await _db.QrScanLogs
            .Include(l => l.Restaurant)
            .OrderByDescending(l => l.ScannedAt)
            .Take(10)
            .ToListAsync();
    }

    public static DateTime ToVietnamTime(DateTime value)
    {
        return value.Kind == DateTimeKind.Local
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc).AddHours(7);
    }

    public record TopPoiItem(string Name, int Count, int Percent);
    public record SupportedLanguageItem(string Code, string Name);
}
