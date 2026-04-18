// Pages/Pois/Edit.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Pois;

[Authorize]
public class EditModel : PageModel
{
    private readonly RestaurantService _restaurantService;

    [BindProperty]
    public Restaurant Poi { get; set; } = new();

    public EditModel(RestaurantService restaurantService) => _restaurantService = restaurantService;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var poi = await _restaurantService.GetByIdAsync(id);
        if (poi is null) return NotFound();
        Poi = poi;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _restaurantService.UpdateAsync(Poi);
        TempData["Message"] = "Đã lưu thay đổi.";
        return RedirectToPage("Index");
    }
}