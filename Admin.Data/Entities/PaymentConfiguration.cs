using Admin.Data.Enums;

namespace Admin.Data.Entities;

/// <summary>
/// Encrypted payment gateway credentials for a business (one active gateway per business).
/// </summary>
public class PaymentConfiguration
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public PaymentGatewayProvider Gateway { get; set; }

    /// <summary>AES-encrypted JSON payload (gateway-specific keys).</summary>
    public string EncryptedJson { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
