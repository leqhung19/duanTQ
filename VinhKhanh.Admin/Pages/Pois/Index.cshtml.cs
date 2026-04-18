using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Pois;

[Authorize]
public class IndexModel : PageModel
{
    private readonly PoiService _poiService;
    public List<Poi> Pois { get; set; } = [];

    public IndexModel(PoiService poiService) => _poiService = poiService;

    public async Task OnGetAsync()
    {
        // Admin thấy tất cả, Owner chỉ thấy của mình
        var ownerId = User.IsInRole("Admin")
            ? null
            : User.FindFirstValue(ClaimTypes.NameIdentifier);

        Pois = await _poiService.GetAllAsync(ownerId);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _poiService.DeleteAsync(id);
        return RedirectToPage();
    }
}