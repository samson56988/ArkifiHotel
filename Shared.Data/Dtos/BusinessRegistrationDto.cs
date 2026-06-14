namespace Shared.Data.Dtos;

/// <summary>Business registration returned to clients (no password fields).</summary>
public sealed class BusinessRegistrationDto
{
    public Guid Id { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    /// <summary>"Active" or "Inactive".</summary>
    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset TermsAcceptedAt { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? LogoUrl { get; set; }
}
