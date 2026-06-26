namespace Shared.Data.Dtos;

/// <summary>Organization user summary (no secrets).</summary>
public sealed class UserOrganizationDto
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsSuperAdmin { get; set; }

    public bool IsEmailVerified { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
