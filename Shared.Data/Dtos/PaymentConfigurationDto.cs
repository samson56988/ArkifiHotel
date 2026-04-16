namespace Shared.Data.Dtos;

public sealed class PaymentConfigurationDto
{
    /// <summary>None, Paystack, or Flutterwave.</summary>
    public string Provider { get; set; } = "None";

    public bool HasSecretKey { get; set; }
}
