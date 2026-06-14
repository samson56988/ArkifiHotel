namespace Admin.Data.Enums;

/// <summary>Which payment processor the business uses for collecting guest payments.</summary>
public enum PaymentGatewayProvider
{
    None = 0,
    Paystack = 1,
    Flutterwave = 2,
    Monify = 3,
}
