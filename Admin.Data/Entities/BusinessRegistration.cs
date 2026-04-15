using Admin.Data.Enums;

namespace Admin.Data.Entities;

/// <summary>
/// Stores business (hotel / shortlet) registration details for the admin system.
/// </summary>
public class BusinessRegistration
{
    public Guid Id { get; set; }

    /// <summary>Legal or trading / property name shown to guests.</summary>
    public string BusinessName { get; set; } = null!;

    /// <summary>Primary contact email for the account.</summary>
    public string ContactEmail { get; set; } = null!;

    /// <summary>Password hash (e.g. BCrypt, Argon2, ASP.NET Identity password hash). Never store plain text.</summary>
    public string HashedPassword { get; set; } = null!;

    /// <summary>Whether the contact email has been verified.</summary>
    public bool IsEmailVerified { get; set; }

    public string? ContactPhone { get; set; }

    public BusinessRegistrationStatus Status { get; set; }

    /// <summary>When the user accepted terms at registration (UTC).</summary>
    public DateTimeOffset TermsAcceptedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Optional internal notes for admin staff.</summary>
    public string? AdminNotes { get; set; }

    /// <summary>Optional storefront slug (e.g. arkifihub.com/your-slug).</summary>
    public string? Slug { get; set; }
}
