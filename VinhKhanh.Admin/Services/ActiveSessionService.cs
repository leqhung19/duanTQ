using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;

namespace VinhKhanh.Admin.Services;

public class ActiveSessionService
{
    private readonly AppDbContext _db;
    public const int ActiveWindowSeconds = 25;

    public ActiveSessionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> CountActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Database
            .SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS [Value]
                FROM [ActiveSessions]
                WHERE [LastPing] >= DATEADD(SECOND, -25, GETDATE())
                """)
            .SingleAsync(cancellationToken);
    }

    public async Task<int> CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM [ActiveSessions]
            WHERE [LastPing] < DATEADD(SECOND, -25, GETDATE())
            """,
            cancellationToken);
    }
}
