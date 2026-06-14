using System.Text.RegularExpressions;

namespace Admin.Infrastructure.Helpers;

public static partial class BusinessSlugHelper
{
    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "api",
        "auth",
        "book",
        "booking",
        "bookings",
        "dashboard",
        "login",
        "register",
        "www",
    };

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var slug = raw.Trim().ToLowerInvariant();
        slug = slug.Replace(' ', '-');
        slug = Regex.Replace(slug, @"[^a-z0-9-]", string.Empty);
        slug = Regex.Replace(slug, @"-{2,}", "-");
        return slug.Trim('-');
    }

    public static string SuggestFromBusinessName(string? businessName)
    {
        return Normalize(businessName);
    }

    public static bool TryValidate(string? raw, out string slug, out string? errorMessage)
    {
        slug = Normalize(raw);
        errorMessage = null;

        if (slug.Length < 3)
        {
            errorMessage = "Slug must be at least 3 characters (letters, numbers, and hyphens only).";
            return false;
        }

        if (slug.Length > 128)
        {
            errorMessage = "Slug must be 128 characters or fewer.";
            return false;
        }

        if (!SlugPattern().IsMatch(slug))
        {
            errorMessage = "Slug may only contain lowercase letters, numbers, and single hyphens between words.";
            return false;
        }

        if (ReservedSlugs.Contains(slug))
        {
            errorMessage = "This slug is reserved. Choose another.";
            return false;
        }

        return true;
    }
}
