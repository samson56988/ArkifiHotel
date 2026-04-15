namespace Shared.Data.Dtos;

/// <summary>Payload for business (hotel) self-registration from ArkifiHub.</summary>
public sealed class RegisterBusinessRequest
{
    public string BusinessName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Must be true to complete registration (terms acceptance timestamp is stored).</summary>
    public bool AcceptTerms { get; set; }

    public string? ContactPhone { get; set; }
}
