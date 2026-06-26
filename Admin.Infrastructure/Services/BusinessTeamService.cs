using System.Security.Cryptography;
using Admin.Data;
using Admin.Data.Constants;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Data.Dtos;
using Shared.Data.Emails;
using Shared.Services.Abstractions;

namespace Admin.Infrastructure.Services;

public sealed class BusinessTeamService : IBusinessTeamService
{
    private readonly AdminDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<BusinessTeamService> _logger;
    private readonly PasswordHasher<UserOrganization> _passwordHasher = new();

    public BusinessTeamService(
        AdminDbContext db,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer,
        ILogger<BusinessTeamService> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrganizationModuleDefinitionDto>> ListModuleDefinitionsAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var isShortlet = await IsShortletAsync(businessId, cancellationToken).ConfigureAwait(false);
        return OrganizationModuleCodes.ForBusinessType(isShortlet)
            .Select(code => new OrganizationModuleDefinitionDto
            {
                Code = code,
                Label = ToLabel(code),
            })
            .ToList();
    }

    public async Task<IReadOnlyList<BusinessTeamMemberDto>> ListMembersAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.UserOrganizations
            .AsNoTracking()
            .Include(u => u.ModulePermissions)
            .Include(u => u.LocationPermissions)
            .Where(u => u.BusinessRegistrationId == businessId)
            .OrderByDescending(u => u.IsSuperAdmin)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(Map).ToList();
    }

    public async Task<(BusinessTeamMemberDto? Data, string? ErrorCode, string? Message)> CreateMemberAsync(
        Guid businessId,
        CreateBusinessTeamMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (firstName.Length < 2 || lastName.Length < 2)
        {
            return (null, "Validation", "First and last name are required.");
        }

        if (string.IsNullOrEmpty(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return (null, "Validation", "A valid email is required.");
        }

        if (!OrganizationUsernameHelper.TryNormalize(request.Username, out var username, out var usernameError))
        {
            return (null, "Validation", usernameError ?? "Invalid username.");
        }

        var business = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);
        if (business is null)
        {
            return (null, "NotFound", "Business not found.");
        }

        if (string.IsNullOrWhiteSpace(business.Slug))
        {
            return (null, "Validation", "Set a business slug on your profile before inviting team members.");
        }

        if (await _db.UserOrganizations.AnyAsync(
                u => u.BusinessRegistrationId == businessId && u.Email == email,
                cancellationToken)
            .ConfigureAwait(false))
        {
            return (null, "DuplicateEmail", "A team member with this email already exists for your business.");
        }

        if (await _db.UserOrganizations.AnyAsync(
                u => u.BusinessRegistrationId == businessId && u.Username == username,
                cancellationToken)
            .ConfigureAwait(false))
        {
            return (null, "DuplicateUsername", "This username is already taken for your business.");
        }

        var isShortlet = business.BusinessType == BusinessType.Shortlet;
        var moduleCodes = NormalizeModuleSelection(request.HasAllModuleAccess, request.ModuleCodes, isShortlet);
        if (!request.HasAllModuleAccess && moduleCodes.Count == 0)
        {
            return (null, "Validation", "Select at least one module or grant all-module access.");
        }

        if (request.HasAllLocationAccess)
        {
            return (null, "Validation", "Staff must be assigned to specific branches.");
        }

        var locationIds = await NormalizeLocationSelectionAsync(
            businessId,
            request.HasAllLocationAccess,
            request.LocationIds,
            request.DefaultLocationId,
            cancellationToken).ConfigureAwait(false);
        if (locationIds is null)
        {
            return (null, "Validation", "Select at least one branch for this staff member.");
        }

        var tempPassword = OrganizationAccessHelper.GenerateTemporaryPassword();
        var now = DateTimeOffset.UtcNow;
        var member = new UserOrganization
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Username = username,
            HashedPassword = string.Empty,
            IsSuperAdmin = false,
            IsDefaultPassword = true,
            HasAllModuleAccess = request.HasAllModuleAccess,
            HasAllLocationAccess = request.HasAllLocationAccess,
            DefaultLocationId = request.DefaultLocationId,
            IsEmailVerified = true,
            IsActive = true,
            CreatedAt = now,
            LastInviteSentAt = now,
        };
        member.HashedPassword = _passwordHasher.HashPassword(member, tempPassword);

        foreach (var code in moduleCodes)
        {
            member.ModulePermissions.Add(new UserOrganizationModulePermission
            {
                Id = Guid.NewGuid(),
                UserOrganizationId = member.Id,
                ModuleCode = code,
            });
        }

        foreach (var locationId in locationIds)
        {
            member.LocationPermissions.Add(new UserOrganizationLocationPermission
            {
                Id = Guid.NewGuid(),
                UserOrganizationId = member.Id,
                BusinessLocationId = locationId,
            });
        }

        _db.UserOrganizations.Add(member);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await SendInviteEmailAsync(business, member, tempPassword, cancellationToken).ConfigureAwait(false);

        return (Map(member), null, null);
    }

    public async Task<(BusinessTeamMemberDto? Data, string? ErrorCode, string? Message)> UpdateMemberAsync(
        Guid businessId,
        Guid memberId,
        UpdateBusinessTeamMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var member = await _db.UserOrganizations
            .Include(u => u.ModulePermissions)
            .FirstOrDefaultAsync(u => u.Id == memberId && u.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
        {
            return (null, "NotFound", "Team member not found.");
        }

        if (member.IsSuperAdmin)
        {
            return (null, "Validation", "The super-admin account cannot be edited here.");
        }

        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (firstName.Length < 2 || lastName.Length < 2)
        {
            return (null, "Validation", "First and last name are required.");
        }

        if (string.IsNullOrEmpty(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return (null, "Validation", "A valid email is required.");
        }

        if (await _db.UserOrganizations.AnyAsync(
                u => u.BusinessRegistrationId == businessId && u.Email == email && u.Id != memberId,
                cancellationToken)
            .ConfigureAwait(false))
        {
            return (null, "DuplicateEmail", "Another team member already uses this email.");
        }

        var isShortlet = await IsShortletAsync(businessId, cancellationToken).ConfigureAwait(false);
        var moduleCodes = NormalizeModuleSelection(request.HasAllModuleAccess, request.ModuleCodes, isShortlet);
        if (!request.HasAllModuleAccess && moduleCodes.Count == 0)
        {
            return (null, "Validation", "Select at least one module or grant all-module access.");
        }

        if (request.HasAllLocationAccess)
        {
            return (null, "Validation", "Staff must be assigned to specific branches.");
        }

        var locationIds = await NormalizeLocationSelectionAsync(
            businessId,
            request.HasAllLocationAccess,
            request.LocationIds,
            request.DefaultLocationId,
            cancellationToken).ConfigureAwait(false);
        if (locationIds is null)
        {
            return (null, "Validation", "Select at least one branch for this staff member.");
        }

        member.FirstName = firstName;
        member.LastName = lastName;
        member.Email = email;
        member.HasAllModuleAccess = request.HasAllModuleAccess;
        member.HasAllLocationAccess = request.HasAllLocationAccess;
        member.DefaultLocationId = request.DefaultLocationId;
        member.IsActive = request.IsActive;
        member.UpdatedAt = DateTimeOffset.UtcNow;

        _db.UserOrganizationModulePermissions.RemoveRange(member.ModulePermissions);
        member.ModulePermissions.Clear();
        foreach (var code in moduleCodes)
        {
            member.ModulePermissions.Add(new UserOrganizationModulePermission
            {
                Id = Guid.NewGuid(),
                UserOrganizationId = member.Id,
                ModuleCode = code,
            });
        }

        _db.UserOrganizationLocationPermissions.RemoveRange(member.LocationPermissions);
        member.LocationPermissions.Clear();
        foreach (var locationId in locationIds)
        {
            member.LocationPermissions.Add(new UserOrganizationLocationPermission
            {
                Id = Guid.NewGuid(),
                UserOrganizationId = member.Id,
                BusinessLocationId = locationId,
            });
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (Map(member), null, null);
    }

    public async Task<IReadOnlyList<BusinessTeamInviteDto>> ListInvitesAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var business = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (business is null || string.IsNullOrWhiteSpace(business.Slug))
        {
            return [];
        }

        var rows = await _db.UserOrganizations
            .AsNoTracking()
            .Where(u => u.BusinessRegistrationId == businessId && !u.IsSuperAdmin)
            .OrderByDescending(u => u.LastInviteSentAt ?? u.CreatedAt)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(u => MapInvite(u, business.Slug!)).ToList();
    }

    public async Task<(BusinessTeamInviteDto? Data, string? ErrorCode, string? Message)> ResendInviteAsync(
        Guid businessId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var business = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (business is null)
        {
            return (null, "NotFound", "Business not found.");
        }

        if (string.IsNullOrWhiteSpace(business.Slug))
        {
            return (null, "Validation", "Set a business slug on your profile before resending invites.");
        }

        var member = await _db.UserOrganizations
            .FirstOrDefaultAsync(u => u.Id == memberId && u.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
        {
            return (null, "NotFound", "Team member not found.");
        }

        if (member.IsSuperAdmin)
        {
            return (null, "Validation", "The super-admin account does not use invite emails.");
        }

        if (!member.IsActive)
        {
            return (null, "Validation", "Unblock this team member before resending the invite.");
        }

        if (!member.IsDefaultPassword)
        {
            return (null, "Validation", "This team member has already signed in and set their password.");
        }

        var tempPassword = OrganizationAccessHelper.GenerateTemporaryPassword();
        member.HashedPassword = _passwordHasher.HashPassword(member, tempPassword);
        member.IsDefaultPassword = true;
        member.LastInviteSentAt = DateTimeOffset.UtcNow;
        member.UpdatedAt = member.LastInviteSentAt;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await SendInviteEmailAsync(business, member, tempPassword, cancellationToken).ConfigureAwait(false);

        return (MapInvite(member, business.Slug), null, null);
    }

    public async Task<(BusinessTeamMemberDto? Data, string? ErrorCode, string? Message)> SetMemberActiveStatusAsync(
        Guid businessId,
        Guid memberId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var member = await _db.UserOrganizations
            .Include(u => u.ModulePermissions)
            .FirstOrDefaultAsync(u => u.Id == memberId && u.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
        {
            return (null, "NotFound", "Team member not found.");
        }

        if (member.IsSuperAdmin)
        {
            return (null, "Validation", "The super-admin account cannot be blocked.");
        }

        if (member.IsActive == isActive)
        {
            return (Map(member), null, null);
        }

        member.IsActive = isActive;
        member.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (Map(member), null, null);
    }

    private async Task SendInviteEmailAsync(
        BusinessRegistration business,
        UserOrganization member,
        string temporaryPassword,
        CancellationToken cancellationToken)
    {
        try
        {
            var loginId = OrganizationUsernameHelper.FormatStaffLogin(business.Slug!, member.Username!);
            var html = _templateRenderer.Render(
                "StaffInvite",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["BusinessName"] = business.BusinessName,
                    ["FirstName"] = member.FirstName,
                    ["LoginId"] = loginId,
                    ["Username"] = member.Username!,
                    ["TemporaryPassword"] = temporaryPassword,
                    ["LoginUrl"] = "http://localhost:4200/login",
                    ["SupportEmail"] = "info@arkifi.store",
                    ["Year"] = DateTime.UtcNow.Year.ToString(),
                });

            var email = new EmailMessage
            {
                ToEmail = member.Email,
                ToName = $"{member.FirstName} {member.LastName}",
                Subject = $"You're invited to {business.BusinessName} on ArkifiStay",
                HtmlBody = html,
                TextBody =
                    $"Hi {member.FirstName}, you were invited to {business.BusinessName} on ArkifiStay. " +
                    $"Sign in at http://localhost:4200/login using {loginId} and temporary password: {temporaryPassword}. " +
                    "You will be asked to set a new password on first sign-in.",
            };

            await _emailSender.SendAsync(email, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Team member created but invite email failed for {Email}", member.Email);
        }
    }

    private static List<string> NormalizeModuleSelection(
        bool hasAllModuleAccess,
        IReadOnlyList<string> requested,
        bool isShortlet)
    {
        if (hasAllModuleAccess)
        {
            return [];
        }

        return requested
            .Select(c => c.Trim().ToLowerInvariant())
            .Where(c => OrganizationModuleCodes.IsValidForBusiness(c, isShortlet))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<List<Guid>?> NormalizeLocationSelectionAsync(
        Guid businessId,
        bool hasAllLocationAccess,
        IReadOnlyList<Guid> requested,
        Guid? defaultLocationId,
        CancellationToken cancellationToken)
    {
        if (hasAllLocationAccess)
        {
            return [];
        }

        var ids = requested.Distinct().ToList();
        if (ids.Count == 0)
        {
            return null;
        }

        var validCount = await _db.BusinessLocations
            .AsNoTracking()
            .CountAsync(l => l.BusinessRegistrationId == businessId && ids.Contains(l.Id), cancellationToken)
            .ConfigureAwait(false);

        if (validCount != ids.Count)
        {
            return null;
        }

        if (defaultLocationId.HasValue && !ids.Contains(defaultLocationId.Value))
        {
            return null;
        }

        return ids;
    }

    private async Task<bool> IsShortletAsync(Guid businessId, CancellationToken cancellationToken) =>
        await _db.BusinessRegistrations
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(b => b.BusinessType == BusinessType.Shortlet)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    private static BusinessTeamInviteDto MapInvite(UserOrganization user, string businessSlug) =>
        new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Username = user.Username,
            StaffLoginId = OrganizationUsernameHelper.FormatStaffLogin(businessSlug, user.Username!),
            IsActive = user.IsActive,
            IsPending = user.IsDefaultPassword,
            InvitedAt = user.CreatedAt,
            LastInviteSentAt = user.LastInviteSentAt ?? user.CreatedAt,
        };

    private static BusinessTeamMemberDto Map(UserOrganization user) =>
        new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Username = user.Username,
            IsSuperAdmin = user.IsSuperAdmin,
            IsDefaultPassword = user.IsDefaultPassword,
            HasAllModuleAccess = user.HasAllModuleAccess,
            HasAllLocationAccess = user.HasAllLocationAccess,
            DefaultLocationId = user.DefaultLocationId,
            IsActive = user.IsActive,
            ModuleCodes = user.ModulePermissions.Select(p => p.ModuleCode).OrderBy(c => c).ToList(),
            LocationIds = user.LocationPermissions.Select(p => p.BusinessLocationId).OrderBy(id => id).ToList(),
            CreatedAt = user.CreatedAt,
        };

    private static string ToLabel(string code) => code switch
    {
        OrganizationModuleCodes.Dashboard => "Overview",
        OrganizationModuleCodes.Rooms => "Rooms / apartments",
        OrganizationModuleCodes.Locations => "Locations",
        OrganizationModuleCodes.Amenities => "Amenities",
        OrganizationModuleCodes.Facilities => "Facilities",
        OrganizationModuleCodes.EventHalls => "Event halls",
        OrganizationModuleCodes.RestaurantMenu => "Restaurant & menu",
        OrganizationModuleCodes.RestaurantOrders => "Restaurant orders",
        OrganizationModuleCodes.Bookings => "Bookings",
        OrganizationModuleCodes.PaymentConfiguration => "Payment configuration",
        OrganizationModuleCodes.Customers => "Customers",
        OrganizationModuleCodes.BookingPayments => "Booking payments",
        OrganizationModuleCodes.Subscription => "Subscription",
        OrganizationModuleCodes.Profile => "Profile",
        OrganizationModuleCodes.SocialProfile => "Social & contact",
        OrganizationModuleCodes.StorefrontDesigner => "Storefront designer",
        _ => code,
    };
}
