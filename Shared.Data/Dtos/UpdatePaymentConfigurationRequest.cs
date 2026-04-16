namespace Shared.Data.Dtos;

public sealed class UpdatePaymentConfigurationRequest
{
    /// <summary>None, Paystack, or Flutterwave (case-insensitive).</summary>
    public string Provider { get; set; } = "None";

    /// <summary>
    /// Paystack or Flutterwave secret key. Leave empty to keep the existing key when the provider is unchanged.
    /// Required when switching providers or enabling payments for the first time.
    /// </summary>
    public string? SecretKey { get; set; }
}
