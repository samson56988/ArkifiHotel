using System.Security.Claims;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Admin.Infrastructure.Services;

public sealed class OrganizationUserContext : IOrganizationUserContext
{
    public OrganizationUserContext(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        BusinessId = OrganizationClaimsHelper.GetBusinessId(user);
        UserOrganizationId = OrganizationClaimsHelper.GetUserId(user);
        IsSuperAdmin = OrganizationClaimsHelper.IsSuperAdmin(user);
        HasAllLocationAccess = OrganizationClaimsHelper.HasAllLocationAccess(user);
        LocationIds = OrganizationClaimsHelper.GetLocationIds(user);
    }

    public Guid? BusinessId { get; }

    public Guid? UserOrganizationId { get; }

    public bool IsSuperAdmin { get; }

    public bool HasAllLocationAccess { get; }

    public IReadOnlyList<Guid> LocationIds { get; } = [];

    public bool CanAccessLocation(Guid? locationId)
    {
        if (!locationId.HasValue)
        {
            return HasAllLocationAccess || IsSuperAdmin;
        }

        if (IsSuperAdmin || HasAllLocationAccess)
        {
            return true;
        }

        return LocationIds.Contains(locationId.Value);
    }
}
