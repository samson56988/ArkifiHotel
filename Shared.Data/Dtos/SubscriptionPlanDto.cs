namespace Shared.Data.Dtos;

public sealed class SubscriptionPlanDto
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
}
