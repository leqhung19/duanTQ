using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinhKhanh.Admin.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private const string DefaultAdminEmail = "admin@vinhkhanh.local";
    private readonly UserManager<IdentityUser> _userManager;

    public List<UserViewModel> Users { get; set; } = [];

    public UsersModel(UserManager<IdentityUser> userManager)
        => _userManager = userManager;

    public async Task OnGetAsync()
    {
        var users = await _userManager.GetUsersInRoleAsync("Admin");
        Users = users
            .OrderBy(u => u.Email)
            .Select(u => new UserViewModel(
                u.Id,
                u.Email ?? u.UserName ?? "",
                string.Equals(u.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<IActionResult> OnPostCreateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["Error"] = "Vui long nhap email va mat khau.";
            return RedirectToPage();
        }

        var existing = await _userManager.FindByEmailAsync(email.Trim());
        if (existing is not null)
        {
            if (!await _userManager.IsInRoleAsync(existing, "Admin"))
                await _userManager.AddToRoleAsync(existing, "Admin");

            TempData["Message"] = $"Da cap quyen Admin cho {email}.";
            return RedirectToPage();
        }

        var user = new IdentityUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToPage();
        }

        await _userManager.AddToRoleAsync(user, "Admin");
        TempData["Message"] = $"Da tao tai khoan admin {email}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return RedirectToPage();

        if (string.Equals(user.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Khong the xoa tai khoan admin mac dinh.";
            return RedirectToPage();
        }

        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        if (adminUsers.Count <= 1)
        {
            TempData["Error"] = "Can giu lai it nhat mot tai khoan admin.";
            return RedirectToPage();
        }

        await _userManager.DeleteAsync(user);
        TempData["Message"] = "Da xoa tai khoan admin.";
        return RedirectToPage();
    }

    public record UserViewModel(string Id, string Email, bool IsProtected);
}
