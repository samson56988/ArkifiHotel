namespace Shared.Data.Dtos;

public sealed class PaymentConfigurationDto
{
    /// <summary>None, Paystack, Flutterwave, or Monify.</summary>
    public string Provider { get; set; } = "None";

    public bool IsConfigured { get; set; }

    public bool HasSecretKey { get; set; }

    public bool HasApiKey { get; set; }

    public bool HasContractCode { get; set; }
}
