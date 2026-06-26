using Admin.Data.Constants;
using Admin.Data.Entities;
using Admin.Data.Enums;

namespace Admin.Infrastructure.Helpers;

public static class OrganizationAccessHelper
{
    public static IReadOnlyList<string> ResolveModuleCodes(UserOrganization user, bool isShortlet)
    {
        if (user.IsSuperAdmin || user.HasAllModuleAccess)
        {
            var all = OrganizationModuleCodes.ForBusinessType(isShortlet).ToList();
            all.Add(OrganizationModuleCodes.Team);
            all.Add(OrganizationModuleCodes.Audit);
            return all;
        }

        return user.ModulePermissions
            .Select(p => p.ModuleCode)
            .Where(code => OrganizationModuleCodes.IsValidForBusiness(code, isShortlet))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool HasModuleAccess(UserOrganization user, bool isShortlet, string moduleCode)
    {
        if (user.IsSuperAdmin)
        {
            return true;
        }

        if (OrganizationModuleCodes.SuperAdminOnly.Contains(moduleCode, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        if (user.HasAllModuleAccess)
        {
            return OrganizationModuleCodes.IsValidForBusiness(moduleCode, isShortlet);
        }

        return user.ModulePermissions.Any(
            p => string.Equals(p.ModuleCode, moduleCode, StringComparison.OrdinalIgnoreCase));
    }

    public static string GenerateTemporaryPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$";
        var chars = new char[12];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = alphabet[Random.Shared.Next(alphabet.Length)];
        }

        return new string(chars);
    }
}
