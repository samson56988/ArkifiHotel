using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArkifiHotel.Extensions;

public static class ClaimsExtensions
{
    public static Guid? GetBusinessId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
