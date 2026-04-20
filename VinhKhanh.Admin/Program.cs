using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
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

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/", "AdminOnly");
    o.Conventions.AuthorizeAreaFolder("Identity", "/Account", "AdminOnly");
    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    o.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied");
    o.Conventions.AllowAnonymousToPage("/Qr/Open");
});
builder.Services.AddControllers();

// Dịch vụ nghiệp vụ
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<SyncService>();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<AudioService>();

// Dọn session chết mỗi 2 phút
builder.Services.AddHostedService<SessionCleanupService>();

// CORS cho Mobile App gọi API
builder.Services.AddCors(o => o.AddPolicy("MobileApp", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Khoi tao DB va seed Admin mac dinh
using (var scope = app.Services.CreateScope())
{
    await DatabaseBootstrapper.InitializeAsync(scope.ServiceProvider);
}

app.UseCors("MobileApp");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
