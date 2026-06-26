namespace Shared.Data.Dtos;

public sealed class OrganizationModuleDefinitionDto
{
    public string Code { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public sealed class BusinessTeamMemberDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Username { get; set; }

    public bool IsSuperAdmin { get; set; }

    public bool IsDefaultPassword { get; set; }

    public bool HasAllModuleAccess { get; set; }

    public bool HasAllLocationAccess { get; set; }

    public Guid? DefaultLocationId { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyList<string> ModuleCodes { get; set; } = Array.Empty<string>();

    public IReadOnlyList<Guid> LocationIds { get; set; } = Array.Empty<Guid>();

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CreateBusinessTeamMemberRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public bool HasAllModuleAccess { get; set; }

    public bool HasAllLocationAccess { get; set; }

    public Guid? DefaultLocationId { get; set; }

    public IReadOnlyList<string> ModuleCodes { get; set; } = Array.Empty<string>();

    public IReadOnlyList<Guid> LocationIds { get; set; } = Array.Empty<Guid>();
}

public sealed class UpdateBusinessTeamMemberRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool HasAllModuleAccess { get; set; }

    public bool HasAllLocationAccess { get; set; }

    public Guid? DefaultLocationId { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyList<string> ModuleCodes { get; set; } = Array.Empty<string>();

    public IReadOnlyList<Guid> LocationIds { get; set; } = Array.Empty<Guid>();
}

public sealed class SetBusinessTeamMemberStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class BusinessTeamInviteDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Username { get; set; }

    public string StaffLoginId { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    /// <summary>True when the member has not completed first sign-in password change.</summary>
    public bool IsPending { get; set; }

    public DateTimeOffset InvitedAt { get; set; }

    public DateTimeOffset LastInviteSentAt { get; set; }
}

public sealed class ChangeDefaultPasswordRequest
{
    /// <summary>Email or business-slug/username.</summary>
    public string Login { get; set; } = string.Empty;

    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
