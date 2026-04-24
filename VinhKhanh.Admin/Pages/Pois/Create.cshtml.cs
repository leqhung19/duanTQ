using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Pois;

[Authorize]
public class CreateModel : PageModel
{
    private readonly RestaurantService _svc;
    private readonly IWebHostEnvironment _env;

    [BindProperty]
    public Restaurant Restaurant { get; set; } = new();

    public CreateModel(RestaurantService svc, IWebHostEnvironment env)
    {
        _svc = svc;
        _env = env;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(IFormFile? ImageFile)
    {
        if (Restaurant.Latitude == 0 && Restaurant.Longitude == 0)
            ModelState.AddModelError("", "Vui lòng chọn vị trí trên bản đồ.");

        if (!ModelState.IsValid) return Page();

        // Upload ảnh
        if (ImageFile is { Length: > 0 })
        {
            var folder = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
            using var stream = new FileStream(
                Path.Combine(folder, fileName), FileMode.Create);
            await ImageFile.CopyToAsync(stream);
            Restaurant.Image = fileName;
        }

        await _svc.CreateAsync(Restaurant);
        TempData["Message"] = $"Đã thêm điểm \"{Restaurant.Name}\" thành công. Bản dịch đã được tự động tạo.";
        return RedirectToPage("Index");
    }
}