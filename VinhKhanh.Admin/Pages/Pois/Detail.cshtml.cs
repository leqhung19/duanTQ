/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Pages.Pois;

[Authorize]
public class DetailModel : PageModel
{
    private readonly RestaurantService _restaurantService;
    
    private readonly AudioService _audioService; 

    public Restaurant? Poi { get; set; }
    
    public List<AudioFile> AudioFiles { get; set; } = []; 

    public DetailModel(RestaurantService restaurantService, AudioService audioService)
    {
        _poiService = poiService;
        _audioService = audioService;
    }

    public async Task OnGetAsync(int id)
    {
        Poi = await _poiService.GetByIdAsync(id);
        AudioFiles = await _audioService.GetByPoiAsync(id);
    }

    // Upload audio
    public async Task<IActionResult> OnPostUploadAsync(
        int poiId, string language, IFormFile file)
    {
        var result = await _audioService.UploadAsync(poiId, language, file);
        TempData[result is not null ? "Message" : "Error"] =
            result is not null ? "Upload thành công!" : "Upload thất bại — kiểm tra định dạng và kích thước file.";

        return RedirectToPage(new { id = poiId });
    }

    // Duyệt / huỷ duyệt audio
    public async Task<IActionResult> OnPostPublishAsync(int audioId, bool publish, int id)
    {
        await _audioService.SetPublishedAsync(audioId, publish);
        TempData["Message"] = publish ? "Đã duyệt audio." : "Đã huỷ duyệt.";
        return RedirectToPage(new { id });
    }

    // Xóa audio
    public async Task<IActionResult> OnPostDeleteAudioAsync(int audioId, int id)
    {
        await _audioService.DeleteAsync(audioId);
        TempData["Message"] = "Đã xóa file audio.";
        return RedirectToPage(new { id });
    }
} */