// Pages/Admin/Users.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace VinhKhanh.Admin.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public List<UserViewModel> Users { get; set; } = [];

    public UsersModel(UserManager<IdentityUser> userManager)
        => _userManager = userManager;

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            Users.Add(new UserViewModel(u.Id, u.Email ?? "", roles.ToList()));
        }
    }

    public async Task<IActionResult> OnPostCreateAsync(
        string email, string password, string role)
    {
        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(", ",
                result.Errors.Select(e => e.Description));
            return RedirectToPage();
        }

        await _userManager.AddToRoleAsync(user, role);
        TempData["Message"] = $"Đã tạo tài khoản {email} với vai trò {role}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
            await _userManager.DeleteAsync(user);

        TempData["Message"] = "Đã xóa tài khoản.";
        return RedirectToPage();
    }

    public record UserViewModel(string Id, string Email, List<string> Roles);
}