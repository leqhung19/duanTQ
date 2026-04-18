using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Services;

public class RestaurantService
{
    private readonly AppDbContext _db;
    private readonly TranslationService _trans;

    public RestaurantService(AppDbContext db, TranslationService trans)
    {
        _db = db;
        _trans = trans;
    }

    public async Task<List<Restaurant>> GetAllAsync() =>
        await _db.Restaurants
            .Include(r => r.Category)
            .Include(r => r.ListenLogs)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

    public async Task<Restaurant?> GetByIdAsync(int id) =>
        await _db.Restaurants
            .Include(r => r.Category)
            .Include(r => r.QRCodes)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Restaurant> CreateAsync(Restaurant r)
    {
        // Tự dịch Description
        await _trans.AutoFillTranslationsAsync(
            r.Description_vi, r.Description_en, r.Description_cn,
            t => r.Description_vi = t,
            t => r.Description_en = t,
            t => r.Description_cn = t);

        // Tự dịch AudioContent
        await _trans.AutoFillTranslationsAsync(
            r.AudioContent_vi, r.AudioContent_en, r.AudioContent_cn,
            t => r.AudioContent_vi = t,
            t => r.AudioContent_en = t,
            t => r.AudioContent_cn = t);

        r.CreatedAt = r.UpdatedAt = DateTime.Now;
        _db.Restaurants.Add(r);
        await _db.SaveChangesAsync();

        // Tự tạo QR code
        _db.QRCodes.Add(new QRCode
        {
            RestaurantId = r.Id,
            QRContent = $"VK:{r.Id}"
        });
        await _db.SaveChangesAsync();
        return r;
    }

    public async Task<bool> UpdateAsync(Restaurant r)
    {
        var existing = await _db.Restaurants.FindAsync(r.Id);
        if (existing is null) return false;

        // Tự dịch nếu có ngôn ngữ mới
        await _trans.AutoFillTranslationsAsync(
            r.Description_vi, r.Description_en, r.Description_cn,
            t => r.Description_vi = t,
            t => r.Description_en = t,
            t => r.Description_cn = t);

        await _trans.AutoFillTranslationsAsync(
            r.AudioContent_vi, r.AudioContent_en, r.AudioContent_cn,
            t => r.AudioContent_vi = t,
            t => r.AudioContent_en = t,
            t => r.AudioContent_cn = t);

        existing.Name = r.Name;
        existing.CategoryId = r.CategoryId;
        existing.Image = r.Image;
        existing.Description_vi = r.Description_vi;
        existing.Description_en = r.Description_en;
        existing.Description_cn = r.Description_cn;
        existing.Address = r.Address;
        existing.Phone = r.Phone;
        existing.OpenTime = r.OpenTime;
        existing.PriceRange = r.PriceRange;
        existing.Latitude = r.Latitude;
        existing.Longitude = r.Longitude;
        existing.RadiusMeters = r.RadiusMeters;
        existing.Priority = r.Priority;
        existing.AudioContent_vi = r.AudioContent_vi;
        existing.AudioContent_en = r.AudioContent_en;
        existing.AudioContent_cn = r.AudioContent_cn;
        existing.IsActive = r.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var r = await _db.Restaurants.FindAsync(id);
        if (r is null) return false;
        _db.Restaurants.Remove(r);
        await _db.SaveChangesAsync();
        return true;
    }

    // Dữ liệu cho Mobile App đồng bộ
    public async Task<List<Restaurant>> GetActiveForSyncAsync() =>
        await _db.Restaurants
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();
}