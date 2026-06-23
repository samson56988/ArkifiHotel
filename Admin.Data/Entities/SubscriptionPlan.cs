using Admin.Data.Enums;

namespace Admin.Data.Entities;

/// <summary>Platform subscription catalog (Free, Pro monthly/yearly).</summary>
public class SubscriptionPlan
{
    public Guid Id { get; set; }

    /// <summary>Stable code, e.g. free, pro-monthly, pro-yearly.</summary>
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public SubscriptionPlanTier Tier { get; set; }

    public SubscriptionBillingInterval BillingInterval { get; set; }

    public decimal PriceAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    /// <summary>Percent saved vs paying monthly for 12 months (shown on yearly plan).</summary>
    public int? YearlyDiscountPercent { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
