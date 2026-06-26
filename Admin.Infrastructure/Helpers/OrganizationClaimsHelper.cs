using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Admin.Infrastructure.Helpers;

public static class OrganizationClaimsHelper
{
    public static Guid? GetBusinessId(ClaimsPrincipal user)
    {
        var businessId = user.FindFirstValue("business_id");
        if (Guid.TryParse(businessId, out var parsed))
        {
            return parsed;
        }

        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue("user_id");
        return Guid.TryParse(userId, out var id) ? id : null;
    }

    public static bool IsSuperAdmin(ClaimsPrincipal user) =>
        string.Equals(user.FindFirstValue("is_super_admin"), "true", StringComparison.OrdinalIgnoreCase);

    public static bool HasAllLocationAccess(ClaimsPrincipal user) =>
        IsSuperAdmin(user)
        || string.Equals(user.FindFirstValue("all_locations"), "true", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<Guid> GetLocationIds(ClaimsPrincipal user)
    {
        if (HasAllLocationAccess(user))
        {
            return [];
        }

        var raw = user.FindFirstValue("location_ids");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
    }
}
