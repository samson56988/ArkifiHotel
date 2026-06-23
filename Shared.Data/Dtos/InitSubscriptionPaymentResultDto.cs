namespace Shared.Data.Dtos;

public sealed class InitSubscriptionPaymentResultDto
{
    public string PaymentReference { get; set; } = string.Empty;

    public string PaymentUrl { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string PlanCode { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;
}
