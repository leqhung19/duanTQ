using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Poi> Pois { get; set; }
    public DbSet<AudioFile> AudioFiles { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<ListenLog> ListenLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Index để truy vấn nhanh theo vị trí địa lý
        builder.Entity<Poi>()
            .HasIndex(p => new { p.Latitude, p.Longitude });

        // Index để tìm audio theo POI + ngôn ngữ
        builder.Entity<AudioFile>()
            .HasIndex(a => new { a.PoiId, a.Language });

        // Index analytics theo thời gian
        builder.Entity<ListenLog>()
            .HasIndex(l => l.ListenedAt);

        // Unique: mỗi POI chỉ có 1 bản dịch mỗi ngôn ngữ
        builder.Entity<Translation>()
            .HasIndex(t => new { t.PoiId, t.Language })
            .IsUnique();
    }
}