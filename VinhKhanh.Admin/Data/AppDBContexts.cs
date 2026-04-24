using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Models;

namespace VinhKhanh.Admin.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<QRCode> QRCodes { get; set; }
    public DbSet<AudioFile> AudioFiles { get; set; }
    public DbSet<ListenLog> ListenLogs { get; set; }
    public DbSet<QrScanLog> QrScanLogs { get; set; }
    public DbSet<ActiveSession> ActiveSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Restaurant>()
         .HasIndex(r => new { r.Latitude, r.Longitude });

        b.Entity<Restaurant>()
         .HasIndex(r => r.QrSlug)
         .IsUnique()
         .HasFilter("[QrSlug] IS NOT NULL");

        b.Entity<ListenLog>()
         .HasIndex(l => l.ListenedAt);

        b.Entity<ListenLog>()
         .HasIndex(l => l.AnonymousSessionId);

        b.Entity<QrScanLog>()
         .HasIndex(l => l.ScannedAt);

        b.Entity<QrScanLog>()
         .HasIndex(l => l.RestaurantId);

        b.Entity<QrScanLog>()
         .HasIndex(l => l.AnonymousSessionId);

        b.Entity<AudioFile>()
         .HasIndex(a => new { a.RestaurantId, a.Language });

        b.Entity<ActiveSession>()
         .HasIndex(s => s.ConnectionId).IsUnique();

        // Map đúng tên bảng trong SQL
        b.Entity<QRCode>().ToTable("QRCodes");
        b.Entity<AudioFile>().ToTable("AudioFiles");
        b.Entity<QrScanLog>().ToTable("QrScanLogs");
    }
}
