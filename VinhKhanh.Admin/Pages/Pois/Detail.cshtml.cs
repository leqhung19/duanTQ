using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Pois;

[Authorize(Roles = "Admin")]
public class DetailModel : PageModel
{
    private readonly RestaurantService _restaurantService;
    private readonly AudioService _audioService;

    public Restaurant? Poi { get; set; }
    public List<AudioFile> AudioFiles { get; set; } = [];

    public DetailModel(RestaurantService restaurantService, AudioService audioService)
    {
        _restaurantService = restaurantService;
        _audioService = audioService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Poi = await _restaurantService.GetByIdAsync(id);
        if (Poi is null) return NotFound();

        AudioFiles = await _audioService.GetByRestaurantAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(int id, string language, IFormFile file)
    {
        var (audio, error) = await _audioService.UploadAsync(id, language, file);
        if (audio is null)
            TempData["Error"] = error ?? "Upload file thuyết minh thất bại.";
        else
            TempData["Message"] = "Đã upload file thuyết minh.";

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostPublishAsync(int id, int audioId, bool publish)
    {
        var ok = await _audioService.SetPublishedAsync(audioId, publish);
        TempData[ok ? "Message" : "Error"] = ok
            ? (publish ? "Đã bật đồng bộ file thuyết minh." : "Đã ẩn file thuyết minh khỏi API sync.")
            : "Không tìm thấy file thuyết minh.";

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAudioAsync(int id, int audioId)
    {
        var ok = await _audioService.DeleteAsync(audioId);
        TempData[ok ? "Message" : "Error"] = ok ? "Đã xóa file thuyết minh." : "Không tìm thấy file thuyết minh.";
        return RedirectToPage(new { id });
    }
}
