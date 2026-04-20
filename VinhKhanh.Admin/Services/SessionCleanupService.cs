using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;

namespace VinhKhanh.Admin.Services;

// Chạy nền mỗi 2 phút — xóa session không ping trong 3 phút (app crash)
public class SessionCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCleanupService> _logger;

    public SessionCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var deadline = DateTime.Now.AddMinutes(-3); // Không ping trong 3 phút = chết
            var dead = await db.ActiveSessions
                .Where(s => s.LastPing < deadline)
                .ToListAsync(stoppingToken);

            if (dead.Count > 0)
            {
                db.ActiveSessions.RemoveRange(dead);
                await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Đã dọn {Count} session chết", dead.Count);
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
