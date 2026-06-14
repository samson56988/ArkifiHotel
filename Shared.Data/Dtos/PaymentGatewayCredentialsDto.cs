namespace Shared.Data.Dtos;

/// <summary>Decrypted gateway credentials for server-side payment calls only.</summary>
public sealed class PaymentGatewayCredentialsDto
{
    public string Provider { get; set; } = string.Empty;

    public string? SecretKey { get; set; }

    public string? ApiKey { get; set; }

    public string? ContractCode { get; set; }
}
