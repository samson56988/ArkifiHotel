namespace Shared.Data.Dtos;

public sealed class UpdatePaymentConfigurationRequest
{
    /// <summary>None, Paystack, Flutterwave, or Monify (case-insensitive).</summary>
    public string Provider { get; set; } = "None";

    /// <summary>Paystack, Flutterwave, or Monify secret key.</summary>
    public string? SecretKey { get; set; }

    /// <summary>Monify API key.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Monify contract code.</summary>
    public string? ContractCode { get; set; }
}
