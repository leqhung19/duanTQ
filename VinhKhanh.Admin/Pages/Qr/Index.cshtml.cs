using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Qr;

[Authorize]
public class IndexModel : PageModel
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp"
    };

    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public List<QrPoiItem> Items { get; set; } = [];
    public int TotalPoi { get; set; }
    public int PoiWithQrImage { get; set; }
    public int PoiWithoutQrImage { get; set; }

    public IndexModel(AppDbContext db, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _db = db;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostRefreshAsync()
    {
        var pois = await _db.Restaurants
            .Include(r => r.QRCodes)
            .OrderBy(r => r.Id)
            .ToListAsync();

        foreach (var poi in pois)
        {
            var qr = poi.QRCodes
                .OrderBy(q => q.Id)
                .FirstOrDefault();

            if (qr is null)
            {
                qr = new QRCode
                {
                    RestaurantId = poi.Id,
                    CreatedAt = DateTime.Now
                };
                _db.QRCodes.Add(qr);
            }

            if (string.IsNullOrWhiteSpace(poi.QrSlug))
                poi.QrSlug = await QrSlugService.EnsureUniqueAsync(_db, poi.QrSlug, poi.Id, poi.Name);

            qr.QRContent = BuildQrUrl(poi);
            qr.IsActive = poi.IsActive;
        }

        await _db.SaveChangesAsync();
        TempData["Message"] = "Đã cập nhật URL QR theo domain hiện tại.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadAsync(int restaurantId, IFormFile? qrImage)
    {
        var poi = await _db.Restaurants
            .Include(r => r.QRCodes)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (poi is null)
        {
            TempData["Error"] = "Không tìm thấy POI.";
            return RedirectToPage();
        }

        if (qrImage is null || qrImage.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn ảnh QR.";
            return RedirectToPage();
        }

        var extension = Path.GetExtension(qrImage.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            TempData["Error"] = "Ảnh QR chỉ hỗ trợ PNG, JPG, JPEG hoặc WEBP.";
            return RedirectToPage();
        }

        var qr = poi.QRCodes
            .OrderBy(q => q.Id)
            .FirstOrDefault();

        var oldImagePath = qr?.ImagePath;
        var folder = Path.Combine(_environment.WebRootPath, "qr", poi.Id.ToString());
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(folder, fileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await qrImage.CopyToAsync(stream);
        }

        if (qr is null)
        {
            qr = new QRCode
            {
                RestaurantId = poi.Id,
                CreatedAt = DateTime.Now
            };
            _db.QRCodes.Add(qr);
        }

        if (string.IsNullOrWhiteSpace(poi.QrSlug))
            poi.QrSlug = await QrSlugService.EnsureUniqueAsync(_db, poi.QrSlug, poi.Id, poi.Name);

        qr.QRContent = BuildQrUrl(poi);
        qr.ImagePath = $"/qr/{poi.Id}/{fileName}";
        qr.IsActive = true;

        await _db.SaveChangesAsync();
        DeleteOldQrImage(oldImagePath, qr.ImagePath);

        TempData["Message"] = $"Đã đổi ảnh QR cho {poi.Name}.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var restaurants = await _db.Restaurants
            .AsNoTracking()
            .Include(r => r.QRCodes)
            .OrderByDescending(r => r.IsActive)
            .ThenByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync();

        Items = restaurants.Select(r =>
        {
            var qr = r.QRCodes
                .OrderByDescending(q => !string.IsNullOrWhiteSpace(q.ImagePath))
                .ThenBy(q => q.Id)
                .FirstOrDefault();

            return new QrPoiItem(
                r.Id,
                r.Name,
                r.Address,
                r.IsActive,
                qr?.Id,
                qr?.ImagePath,
                BuildQrUrl(r),
                BuildQrImageUrl(BuildQrUrl(r)));
        }).ToList();

        TotalPoi = Items.Count;
        PoiWithQrImage = Items.Count(i => i.HasQrImage);
        PoiWithoutQrImage = TotalPoi - PoiWithQrImage;
    }

    private string BuildQrUrl(Restaurant poi)
    {
        var code = string.IsNullOrWhiteSpace(poi.QrSlug)
            ? poi.Id.ToString()
            : poi.QrSlug;

        var configured = _configuration["AppSettings:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return $"{configured.Trim().TrimEnd('/')}/q/{code}";

        var request = HttpContext.Request;
        var scheme = request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto)
            ? forwardedProto.ToString().Split(',')[0].Trim()
            : request.Scheme;
        var host = request.Headers.TryGetValue("X-Forwarded-Host", out var forwardedHost)
            ? forwardedHost.ToString().Split(',')[0].Trim()
            : request.Host.ToString();

        return $"{scheme}://{host}/q/{code}";
    }

    private static string BuildQrImageUrl(string qrContent) =>
        "https://api.qrserver.com/v1/create-qr-code/?size=220x220&data="
        + Uri.EscapeDataString(qrContent);

    private void DeleteOldQrImage(string? oldPath, string? newPath)
    {
        if (string.IsNullOrWhiteSpace(oldPath) || oldPath == newPath)
            return;

        var relativePath = oldPath
            .Replace('\\', '/')
            .TrimStart('/');

        if (!relativePath.StartsWith("qr/", StringComparison.OrdinalIgnoreCase))
            return;

        var physicalPath = Path.Combine(
            _environment.WebRootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(physicalPath))
            System.IO.File.Delete(physicalPath);
    }

    public record QrPoiItem(
        int PoiId,
        string PoiName,
        string? Address,
        bool PoiIsActive,
        int? QrId,
        string? QrImagePath,
        string QrContent,
        string QrImageUrl)
    {
        public bool HasQrImage => !string.IsNullOrWhiteSpace(QrImagePath);
    }
}
