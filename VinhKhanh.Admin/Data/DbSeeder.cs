using Microsoft.AspNetCore.Identity;

namespace VinhKhanh.Admin.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // Web chi cho phep tai khoan quan tri vien.
        string[] roles = ["Admin"];
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Tạo tài khoản Admin mặc định (đổi mật khẩu sau khi deploy)
        var adminEmail = "admin@vinhkhanh.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            await userManager.CreateAsync(admin, "Admin@123456!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
