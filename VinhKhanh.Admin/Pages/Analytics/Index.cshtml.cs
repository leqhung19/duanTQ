using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;

namespace VinhKhanh.Admin.Pages.Analytics;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public int TotalListens { get; set; }
    public int TodayListens { get; set; }
    public int GpsTriggerCount { get; set; }
    public int QrTriggerCount { get; set; }

    public List<PoiStat> TopPois { get; set; } = [];

    public IndexModel(AppDbContext db) => _db = db;

    public async Task OnGetAsync()
    {
        var today = DateTime.UtcNow.Date;

        TotalListens = await _db.ListenLogs.CountAsync();
        TodayListens = await _db.ListenLogs.CountAsync(l => l.ListenedAt >= today);
        GpsTriggerCount = await _db.ListenLogs.CountAsync(l => l.TriggerType == "gps");
        QrTriggerCount = await _db.ListenLogs.CountAsync(l => l.TriggerType == "qr");

        // Lấy tên POI trước
        var poiNames = await _db.Restaurants
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var grouped = await _db.ListenLogs
            .GroupBy(l => l.RestaurantId)
            .Select(g => new {
                PoiId = g.Key,
                Count = g.Count(),
                TopLang = g.GroupBy(x => x.Language)
                           .OrderByDescending(x => x.Count())
                           .Select(x => x.Key)
                           .FirstOrDefault()
            })
            .OrderByDescending(g => g.Count)
            .Take(10)
            .ToListAsync();

        var max = grouped.FirstOrDefault()?.Count ?? 1;

        TopPois = grouped.Select(g => new PoiStat(
            poiNames.GetValueOrDefault(g.PoiId, "Không rõ"),
            g.Count,
            (int)(g.Count * 100.0 / max),
            g.TopLang ?? "vi"
        )).ToList();
    }

    public record PoiStat(string PoiName, int Count, int Percent, string TopLanguage);
}