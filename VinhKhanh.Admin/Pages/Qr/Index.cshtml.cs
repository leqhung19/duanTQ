using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

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

        qr.QRContent = BuildQrUrl(poi.Id);
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
                qr?.QRContent ?? BuildQrUrl(r.Id));
        }).ToList();

        TotalPoi = Items.Count;
        PoiWithQrImage = Items.Count(i => i.HasQrImage);
        PoiWithoutQrImage = TotalPoi - PoiWithQrImage;
    }

    private string BuildQrUrl(int poiId)
    {
        var configured = _configuration["AppSettings:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return $"{configured.Trim().TrimEnd('/')}/q/{poiId}";

        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}/q/{poiId}";
    }

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
        string QrContent)
    {
        public bool HasQrImage => !string.IsNullOrWhiteSpace(QrImagePath);
    }
}
