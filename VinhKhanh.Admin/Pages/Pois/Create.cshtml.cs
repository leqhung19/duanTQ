using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Pois;

[Authorize]
public class CreateModel : PageModel
{
    private readonly PoiService _poiService;

    [BindProperty]
    public Poi Poi { get; set; } = new();

    public CreateModel(PoiService poiService) => _poiService = poiService;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Gán OwnerId là user hiện tại
        Poi.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _poiService.CreateAsync(Poi);
        return RedirectToPage("Index");
    }
}