namespace Shared.Data.Dtos;

public sealed class BusinessSubscriptionPaymentHistoryDto
{
    public Guid Id { get; set; }

    public string PaymentReference { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public string PlanCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    /// <summary>Pending, Completed, or Failed.</summary>
    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
