using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArkifiHotel.Extensions;

public static class ClaimsExtensions
{
    public static Guid? GetBusinessId(this ClaimsPrincipal user)
    {
        var businessId = user.FindFirstValue("business_id");
        if (Guid.TryParse(businessId, out var parsedBusinessId))
        {
            return parsedBusinessId;
        }

        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue("user_id");
        return Guid.TryParse(userId, out var id) ? id : null;
    }

    public static bool IsSuperAdmin(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("is_super_admin");
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasAllLocationAccess(this ClaimsPrincipal user)
    {
        if (IsSuperAdmin(user))
        {
            return true;
        }

        return string.Equals(user.FindFirstValue("all_locations"), "true", StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<Guid> GetLocationIds(this ClaimsPrincipal user)
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
