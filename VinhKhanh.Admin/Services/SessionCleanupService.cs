namespace VinhKhanh.Admin.Services;

// Chạy nền ngắn nhịp để dashboard phản ứng nhanh khi app vào/ra.
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
            var activeSessions = scope.ServiceProvider.GetRequiredService<ActiveSessionService>();
            var deleted = await activeSessions.CleanupExpiredSessionsAsync(stoppingToken);

            if (deleted > 0)
            {
                _logger.LogInformation("Đã dọn {Count} session chết", deleted);
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}
