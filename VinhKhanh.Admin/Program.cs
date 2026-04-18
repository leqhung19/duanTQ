using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Hubs;
using VinhKhanh.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// Chỉ Admin — không cần user thông thường
builder.Services.AddDefaultIdentity<IdentityUser>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Dịch vụ nghiệp vụ
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<SyncService>();
builder.Services.AddScoped<TranslationService>();

// Dọn session chết mỗi 2 phút
builder.Services.AddHostedService<SessionCleanupService>();

// CORS cho Mobile App gọi API
builder.Services.AddCors(o => o.AddPolicy("MobileApp", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Seed Admin mặc định
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    using (var scope2  = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await DbSeeder.SeedRolesAsync(services);
    }
}

app.UseCors("MobileApp");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<AppPresenceHub>("/hubs/presence");

app.Run();