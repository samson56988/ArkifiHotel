namespace Shared.Data.Dtos;

/// <summary>Business account summary returned from login (no secrets).</summary>
public sealed class BusinessAccountDto
{
    public Guid Id { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    /// <summary>"Active" or "Inactive".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Logged-in organization user id.</summary>
    public Guid? UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool IsSuperAdmin { get; set; }

    public string? Username { get; set; }

    public bool HasAllModuleAccess { get; set; }

    public bool RequiresPasswordChange { get; set; }

    /// <summary>Email where login 2FA OTP is sent (useful when login id is a username).</summary>
    public string? TwoFactorEmail { get; set; }

    public IReadOnlyList<string> ModuleCodes { get; set; } = Array.Empty<string>();

    public bool HasAllLocationAccess { get; set; }

    public Guid? DefaultLocationId { get; set; }

    public IReadOnlyList<Guid> LocationIds { get; set; } = Array.Empty<Guid>();
}
