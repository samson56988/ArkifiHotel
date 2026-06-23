namespace Admin.Infrastructure.Options;

public sealed class PaystackOptions
{
    public const string SectionName = "Paystack";

    public string BaseUrl { get; set; } = "https://api.paystack.co";

    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Paystack redirect after checkout, e.g. http://localhost:4200/subscription</summary>
    public string CallbackUrl { get; set; } = "http://localhost:4200/subscription";
}
