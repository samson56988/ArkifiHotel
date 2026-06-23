namespace Shared.Data.Dtos;

public sealed class SubscriptionPlanOptionDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Tier { get; set; } = string.Empty;

    public string BillingInterval { get; set; } = string.Empty;

    public decimal PriceAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public int? YearlyDiscountPercent { get; set; }

    public int SortOrder { get; set; }

    public bool CanSelect { get; set; }

    public bool RequiresPayment { get; set; }

    /// <summary>Current, Upgrade, Downgrade, or Renew.</summary>
    public string ChangeType { get; set; } = "Current";

    public string? DisabledReason { get; set; }
}
