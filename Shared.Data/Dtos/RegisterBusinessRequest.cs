namespace Shared.Data.Dtos;

/// <summary>Payload for business (hotel) self-registration from ArkifiHub.</summary>
public sealed class RegisterBusinessRequest
{
    public string BusinessName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Must be true to complete registration (terms acceptance timestamp is stored).</summary>
    public bool AcceptTerms { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>URL-safe storefront path segment, e.g. marina-suites.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Hotel or Shortlet.</summary>
    public string BusinessType { get; set; } = "Hotel";

    /// <summary>Plan code: free, pro-monthly, or pro-yearly.</summary>
    public string PlanCode { get; set; } = "free";
}
