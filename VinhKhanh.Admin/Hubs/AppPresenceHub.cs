using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Models;
using VinhKhanh.Admin.Services;

namespace VinhKhanh.Admin.Hubs;

public class AppPresenceHub : Hub
{
    private readonly AppDbContext _db;

    public AppPresenceHub(AppDbContext db) => _db = db;

    public override async Task OnConnectedAsync()
    {
        var platform = Context.GetHttpContext()
            ?.Request.Headers["X-Platform"].ToString() ?? "unknown";
        var lang = Context.GetHttpContext()
            ?.Request.Headers["X-Language"].ToString() ?? "vi";

        var now = PresenceClock.Now();

        _db.ActiveSessions.Add(new ActiveSession
        {
            ConnectionId = Context.ConnectionId,
            DevicePlatform = platform,
            Language = lang,
            ConnectedAt = now,
            LastPing = now,
        });
        await _db.SaveChangesAsync();

        var deadline = now.AddMinutes(-3);
        var count = await _db.ActiveSessions.CountAsync(s => s.LastPing >= deadline);
        await Clients.All.SendAsync("ActiveUsersUpdated", count);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var s = await _db.ActiveSessions
            .FirstOrDefaultAsync(s => s.ConnectionId == Context.ConnectionId);
        if (s is not null) { _db.ActiveSessions.Remove(s); await _db.SaveChangesAsync(); }

        var deadline = PresenceClock.Now().AddMinutes(-3);
        var count = await _db.ActiveSessions.CountAsync(s => s.LastPing >= deadline);
        await Clients.All.SendAsync("ActiveUsersUpdated", count);
        await base.OnDisconnectedAsync(ex);
    }

    public async Task Ping()
    {
        var s = await _db.ActiveSessions
            .FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);
        if (s is not null)
        {
            s.LastPing = PresenceClock.Now();
            await _db.SaveChangesAsync();
        }
    }
}
