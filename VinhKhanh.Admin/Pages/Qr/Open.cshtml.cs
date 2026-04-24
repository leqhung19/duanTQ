using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Pages.Qr;

[AllowAnonymous]
public class OpenModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public Restaurant? Poi { get; set; }
    public string AppDeepLink { get; set; } = "";
    public string AndroidDownloadUrl { get; set; } = "";
    public string IosDownloadUrl { get; set; } = "";

    private const string QrVisitorCookieName = "vk_qr_visitor";

    public OpenModel(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task OnGetAsync(string code)
    {
        Response.ContentType = "text/html; charset=utf-8";

        Poi = int.TryParse(code, out var id)
            ? await _db.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive)
            : await _db.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.QrSlug == code && r.IsActive);

        AppDeepLink = Poi is null
            ? ""
            : $"doan://restaurant/{Poi.Id}?api={Uri.EscapeDataString(BuildApiBaseUrl())}";
        AndroidDownloadUrl = _configuration["AppSettings:AndroidDownloadUrl"] ?? "/downloads/vinhkhanh-app.apk";
        IosDownloadUrl = _configuration["AppSettings:IosDownloadUrl"] ?? "#";

        if (Poi is not null)
            await LogQrScanAsync(Poi.Id, code);
    }

    private async Task LogQrScanAsync(int restaurantId, string code)
    {
        _db.QrScanLogs.Add(new QrScanLog
        {
            RestaurantId = restaurantId,
            QrCode = NormalizeQrCode(code),
            DevicePlatform = DetectDevicePlatform(),
            AnonymousSessionId = GetOrCreateAnonymousSessionId(),
            ScannedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    private string GetOrCreateAnonymousSessionId()
    {
        if (Request.Cookies.TryGetValue(QrVisitorCookieName, out var existing)
            && !string.IsNullOrWhiteSpace(existing)
            && existing.Length <= 64)
        {
            return existing;
        }

        var sessionId = Guid.NewGuid().ToString("N");
        Response.Cookies.Append(QrVisitorCookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

        return sessionId;
    }

    private string DetectDevicePlatform()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
            return "android";
        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("iPod", StringComparison.OrdinalIgnoreCase))
            return "ios";
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
            return "windows";
        return "unknown";
    }

    private static string NormalizeQrCode(string code)
    {
        var value = code.Trim();
        return value.Length <= 200 ? value : value[..200];
    }

    private string BuildApiBaseUrl()
    {
        var configured = _configuration["AppSettings:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return $"{configured.Trim().TrimEnd('/')}/api/";

        var scheme = Request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto)
            ? forwardedProto.ToString().Split(',')[0].Trim()
            : Request.Scheme;
        var host = Request.Headers.TryGetValue("X-Forwarded-Host", out var forwardedHost)
            ? forwardedHost.ToString().Split(',')[0].Trim()
            : Request.Host.ToString();

        return $"{scheme}://{host}/api/";
    }
}
