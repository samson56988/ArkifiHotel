using Admin.Data.Enums;

namespace Admin.Data.Entities;

/// <summary>Platform Paystack payment for subscription plan changes.</summary>
public class BusinessSubscriptionPayment
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid TargetPlanId { get; set; }

    public SubscriptionPlan TargetPlan { get; set; } = null!;

    public string PaymentReference { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public BusinessSubscriptionPaymentStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
