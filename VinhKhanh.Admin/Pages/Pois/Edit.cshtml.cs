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
    private readonly IWebHostEnvironment _environment;

    [BindProperty]
    public Restaurant Poi { get; set; } = new();

    public EditModel(RestaurantService restaurantService, IWebHostEnvironment environment)
    {
        _restaurantService = restaurantService;
        _environment = environment;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var poi = await _restaurantService.GetByIdAsync(id);
        if (poi is null) return NotFound();
        Poi = poi;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? ImageFile)
    {
        if (!ModelState.IsValid) return Page();

        if (ImageFile is { Length: > 0 })
        {
            var oldImage = Poi.Image;

            var folder = Path.Combine(_environment.WebRootPath, "images");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(ImageFile.FileName)}";
            var physicalPath = Path.Combine(folder, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            Poi.Image = fileName;
            DeleteOldImage(oldImage, Poi.Image);
        }

        await _restaurantService.UpdateAsync(Poi);
        TempData["Message"] = "Da luu thay doi.";
        return RedirectToPage("Index");
    }

    private void DeleteOldImage(string? oldImage, string? newImage)
    {
        if (string.IsNullOrWhiteSpace(oldImage) || oldImage == newImage)
            return;

        if (Uri.TryCreate(oldImage, UriKind.Absolute, out _))
            return;

        var relativePath = oldImage.Replace('\\', '/').TrimStart('/');
        if (!relativePath.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            relativePath = $"images/{relativePath}";

        var physicalPath = Path.Combine(
            _environment.WebRootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(physicalPath))
            System.IO.File.Delete(physicalPath);
    }
}
