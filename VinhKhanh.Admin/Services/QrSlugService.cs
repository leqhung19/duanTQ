using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using VinhKhanh.Admin.Data;

namespace VinhKhanh.Admin.Services;

public static partial class QrSlugService
{
    public static string Generate(string? text, int fallbackId)
    {
        var value = string.IsNullOrWhiteSpace(text)
            ? $"poi-{fallbackId}"
            : text.Trim().ToLowerInvariant();

        value = value.Replace('đ', 'd');
        value = value.Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }

        var slug = HyphenRegex().Replace(builder.ToString(), "-").Trim('-');
        if (string.IsNullOrWhiteSpace(slug))
            slug = $"poi-{fallbackId}";

        return slug.Length <= 80 ? slug : slug[..80].Trim('-');
    }

    public static async Task<string> EnsureUniqueAsync(AppDbContext db, string? desiredSlug, int restaurantId, string? name)
    {
        var baseSlug = Generate(string.IsNullOrWhiteSpace(desiredSlug) ? name : desiredSlug, restaurantId);
        var slug = baseSlug;
        var suffix = 2;

        while (await db.Restaurants.AnyAsync(r => r.Id != restaurantId && r.QrSlug == slug))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    [GeneratedRegex("-+")]
    private static partial Regex HyphenRegex();
}
