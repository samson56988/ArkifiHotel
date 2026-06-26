using System.Text.RegularExpressions;

namespace Admin.Infrastructure.Helpers;

public static partial class OrganizationUsernameHelper
{
    private static readonly Regex UsernamePattern = UsernameRegex();

    public static bool TryNormalize(string? raw, out string username, out string? error)
    {
        username = string.Empty;
        error = null;

        var trimmed = raw?.Trim().ToLowerInvariant() ?? string.Empty;
        if (trimmed.Length < 3)
        {
            error = "Username must be at least 3 characters.";
            return false;
        }

        if (trimmed.Length > 64)
        {
            error = "Username must be 64 characters or fewer.";
            return false;
        }

        if (!UsernamePattern.IsMatch(trimmed))
        {
            error = "Username may only contain letters, numbers, dots, hyphens, and underscores.";
            return false;
        }

        username = trimmed;
        return true;
    }

    public static bool TryParseStaffLogin(string? raw, out string businessSlug, out string username, out string? error)
    {
        businessSlug = string.Empty;
        username = string.Empty;
        error = null;

        var trimmed = raw?.Trim() ?? string.Empty;
        if (!trimmed.Contains('/'))
        {
            error = "Staff sign-in uses business-slug/username (e.g. lekki-suites/frontdesk).";
            return false;
        }

        var parts = trimmed.Split('/', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            error = "Invalid staff login format.";
            return false;
        }

        if (!BusinessSlugHelper.TryValidate(parts[0], out businessSlug, out var slugError))
        {
            error = slugError ?? "Invalid business slug.";
            return false;
        }

        if (!TryNormalize(parts[1], out username, out var userError))
        {
            error = userError;
            return false;
        }

        return true;
    }

    public static string FormatStaffLogin(string businessSlug, string username) =>
        $"{businessSlug}/{username}";

    [GeneratedRegex("^[a-z0-9][a-z0-9._-]{1,62}[a-z0-9]$|^[a-z0-9]{3}$")]
    private static partial Regex UsernameRegex();
}
