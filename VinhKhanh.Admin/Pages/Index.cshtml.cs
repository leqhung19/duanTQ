using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public int TotalPois { get; set; }
    public int ActivePois { get; set; }
    public int ActiveUsers { get; set; }
    public int TodayListens { get; set; }
    public int TotalListens { get; set; }

    public List<TopPoiItem> TopPois { get; set; } = [];
    public List<LangItem> LanguageStats { get; set; } = [];
    public List<ListenLog> RecentLogs { get; set; } = [];

    public IndexModel(AppDbContext db) => _db = db;

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;

        TotalPois = await _db.Restaurants.CountAsync();
        ActivePois = await _db.Restaurants.CountAsync(r => r.IsActive);
        var activeDeadline = DateTime.Now.AddMinutes(-3);
        ActiveUsers = await _db.ActiveSessions.CountAsync(s => s.LastPing >= activeDeadline);
        TodayListens = await _db.ListenLogs.CountAsync(l => l.ListenedAt >= today);
        TotalListens = await _db.ListenLogs.CountAsync();

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

        // Thống kê ngôn ngữ
        LanguageStats = await _db.ListenLogs
     .GroupBy(l => l.Language)
     .Select(g => new
     {
         Language = g.Key,
         Count = g.Count()
     })
     .OrderByDescending(x => x.Count)
     .Select(x => new LangItem(x.Language, x.Count))
     .ToListAsync();

        // 10 lượt nghe gần nhất
        RecentLogs = await _db.ListenLogs
            .Include(l => l.Restaurant)
            .OrderByDescending(l => l.ListenedAt)
            .Take(10)
            .ToListAsync();
    }

    public record TopPoiItem(string Name, int Count, int Percent);
    public record LangItem(string Lang, int Count);
}
