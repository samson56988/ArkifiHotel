namespace Shared.Data.Dtos;

/// <summary>Public-facing business profile for the authenticated owner.</summary>
public sealed class BusinessProfileDto
{
    public Guid Id { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string? Slug { get; set; }

    /// <summary>Absolute URL to the business logo, if uploaded.</summary>
    public string? LogoUrl { get; set; }

    public string ContactEmail { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Hotel or Shortlet.</summary>
    public string BusinessType { get; set; } = "Hotel";
}
