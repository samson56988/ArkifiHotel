using Admin.Data.Entities;
using Admin.Services.Abstractions;

namespace Admin.Infrastructure.Helpers;

public static class OrganizationLocationHelper
{
    public static IReadOnlyList<Guid> ResolveLocationIds(UserOrganization user)
    {
        if (user.IsSuperAdmin || user.HasAllLocationAccess)
        {
            return [];
        }

        return user.LocationPermissions
            .Select(p => p.BusinessLocationId)
            .Distinct()
            .ToList();
    }

    public static bool HasAllLocationAccess(UserOrganization user) =>
        user.IsSuperAdmin || user.HasAllLocationAccess;

    public static bool CanAccessLocation(UserOrganization user, Guid? locationId)
    {
        if (!locationId.HasValue)
        {
            return HasAllLocationAccess(user);
        }

        if (HasAllLocationAccess(user))
        {
            return true;
        }

        return user.LocationPermissions.Any(p => p.BusinessLocationId == locationId.Value);
    }

    public static bool CanAccessLocation(IOrganizationUserContext context, Guid? locationId)
    {
        if (!locationId.HasValue)
        {
            return context.IsSuperAdmin || context.HasAllLocationAccess;
        }

        return context.CanAccessLocation(locationId);
    }
}
