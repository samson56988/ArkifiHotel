namespace Admin.Data.Entities;

/// <summary>
/// Staff or owner account linked to a business (hotel / shortlet).
/// Exactly one <see cref="IsSuperAdmin"/> user per business — the account creator by default.
/// </summary>
public class UserOrganization
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    /// <summary>Contact email (unique per business). Used for invites and login 2FA.</summary>
    public string Email { get; set; } = null!;

    /// <summary>Staff login name (unique per business). Super-admin signs in with email instead.</summary>
    public string? Username { get; set; }

    public string HashedPassword { get; set; } = null!;

    /// <summary>True for the single super-admin per business (platform owner).</summary>
    public bool IsSuperAdmin { get; set; }

    /// <summary>When true, user must change password after signing in with a temporary password.</summary>
    public bool IsDefaultPassword { get; set; }

    /// <summary>When true (and not super-admin), user can access every module for the business type.</summary>
    public bool HasAllModuleAccess { get; set; }

    /// <summary>When true, user can access every branch. Super-admin always has all branches.</summary>
    public bool HasAllLocationAccess { get; set; }

    /// <summary>Optional default branch for staff UI pickers.</summary>
    public Guid? DefaultLocationId { get; set; }

    public BusinessLocation? DefaultLocation { get; set; }

    public bool IsEmailVerified { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the latest invite email was sent (initial invite or resend).</summary>
    public DateTimeOffset? LastInviteSentAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<UserOrganizationModulePermission> ModulePermissions { get; set; } =
        new List<UserOrganizationModulePermission>();

    public ICollection<UserOrganizationLocationPermission> LocationPermissions { get; set; } =
        new List<UserOrganizationLocationPermission>();
}
