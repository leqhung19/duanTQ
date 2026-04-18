using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Services;

public class PoiService
{
    private readonly AppDbContext _db;

    public PoiService(AppDbContext db) => _db = db;

    // Lấy tất cả POI (Admin thấy hết, Owner chỉ thấy của mình)
    public async Task<List<Poi>> GetAllAsync(string? ownerId = null)
    {
        var query = _db.Pois
            .Include(p => p.AudioFiles)
            .AsQueryable();

        if (ownerId is not null)
            query = query.Where(p => p.OwnerId == ownerId);

        return await query
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    // Lấy 1 POI theo Id (kèm audio + bản dịch)
    public async Task<Poi?> GetByIdAsync(int id)
    {
        return await _db.Pois
            .Include(p => p.AudioFiles)
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Tạo mới POI
    public async Task<Poi> CreateAsync(Poi poi)
    {
        poi.CreatedAt = DateTime.UtcNow;
        poi.UpdatedAt = DateTime.UtcNow;
        _db.Pois.Add(poi);
        await _db.SaveChangesAsync();
        return poi;
    }

    // Cập nhật POI
    public async Task<bool> UpdateAsync(Poi poi)
    {
        var existing = await _db.Pois.FindAsync(poi.Id);
        if (existing is null) return false;

        existing.Name = poi.Name;
        existing.DescriptionVi = poi.DescriptionVi;
        existing.DescriptionEn = poi.DescriptionEn;
        existing.Latitude = poi.Latitude;
        existing.Longitude = poi.Longitude;
        existing.RadiusMeters = poi.RadiusMeters;
        existing.Priority = poi.Priority;
        existing.IsActive = poi.IsActive;
        existing.ImagePath = poi.ImagePath;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    // Xóa POI (soft delete — chỉ tắt IsActive)
    public async Task<bool> DeleteAsync(int id)
    {
        var poi = await _db.Pois.FindAsync(id);
        if (poi is null) return false;

        poi.IsActive = false;
        poi.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // Lấy danh sách POI đang hoạt động cho API sync (mobile)
    public async Task<List<Poi>> GetActivePoisForSyncAsync()
    {
        return await _db.Pois
            .Include(p => p.AudioFiles.Where(a => a.IsPublished))
            .Include(p => p.Translations)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Priority)
            .ToListAsync();
    }
}