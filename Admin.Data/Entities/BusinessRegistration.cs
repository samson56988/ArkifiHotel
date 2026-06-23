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

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    /// <summary>Primary contact email for the account.</summary>
    public string ContactEmail { get; set; } = null!;

    /// <summary>Password hash (e.g. BCrypt, Argon2, ASP.NET Identity password hash). Never store plain text.</summary>
    public string HashedPassword { get; set; } = null!;

    /// <summary>Whether the contact email has been verified.</summary>
    public bool IsEmailVerified { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public BusinessRegistrationStatus Status { get; set; }

    /// <summary>When the user accepted terms at registration (UTC).</summary>
    public DateTimeOffset TermsAcceptedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Optional internal notes for admin staff.</summary>
    public string? AdminNotes { get; set; }

    /// <summary>Optional storefront slug (e.g. arkifistay.com/your-slug).</summary>
    public string? Slug { get; set; }

    /// <summary>Relative path under wwwroot, e.g. uploads/{businessId}/logo/logo.png.</summary>
    public string? LogoPath { get; set; }

    /// <summary>JSON theme for the guest storefront (banner, sections, footer, colors).</summary>
    public string? StorefrontThemeJson { get; set; }

    /// <summary>Hotel or shortlet property type.</summary>
    public BusinessType BusinessType { get; set; }

    /// <summary>Current subscription plan.</summary>
    public Guid SubscriptionPlanId { get; set; }

    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    /// <summary>When the current subscription period ends (UTC).</summary>
    public DateTimeOffset? SubscriptionExpiresAt { get; set; }
}

