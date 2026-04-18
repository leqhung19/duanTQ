using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;
using VinhKhanh.Admin.Services;

var builder = WebApplication.CreateBuilder(args);


// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (có sẵn từ template)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // tắt xác nhận email khi dev
})
.AddRoles<IdentityRole>()           // thêm phân quyền theo role
.AddEntityFrameworkStores<AppDbContext>();

// Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Services
builder.Services.AddScoped<PoiService>();
builder.Services.AddScoped<AudioService>();
builder.Services.AddScoped<SyncService>();

var app = builder.Build();

// Auto migrate khi khởi động (tiện cho dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedRolesAsync(scope.ServiceProvider); // seed role Admin/Owner
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers(); // cho REST API /api/sync

// Debug: in ra tất cả route đã đăng ký
foreach (var item in app.Services
    .GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>()
    .Endpoints)
{
    Console.WriteLine(item.DisplayName);
}


app.Run();