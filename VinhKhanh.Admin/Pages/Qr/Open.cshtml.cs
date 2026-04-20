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

    public OpenModel(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task OnGetAsync(int id)
    {
        Poi = await _db.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        AppDeepLink = $"doan://restaurant/{id}";
        AndroidDownloadUrl = _configuration["AppSettings:AndroidDownloadUrl"] ?? "/downloads/vinhkhanh-app.apk";
        IosDownloadUrl = _configuration["AppSettings:IosDownloadUrl"] ?? "#";
    }
}
