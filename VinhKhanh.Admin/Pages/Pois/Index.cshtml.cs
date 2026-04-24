using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Pois;

[Authorize]
public class IndexModel : PageModel
{
    private readonly RestaurantService _svc;
    public List<Restaurant> Restaurants { get; set; } = [];

    public IndexModel(RestaurantService svc) => _svc = svc;

    public async Task OnGetAsync() =>
        Restaurants = await _svc.GetAllAsync();

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _svc.DeleteAsync(id);
        TempData["Message"] = "Đã xóa điểm.";
        return RedirectToPage();
    }
}