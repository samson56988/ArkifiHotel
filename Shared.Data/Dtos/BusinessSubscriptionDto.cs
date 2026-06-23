namespace Shared.Data.Dtos;

public sealed class BusinessSubscriptionDto
{
    public Guid BusinessId { get; set; }

    public string BusinessType { get; set; } = string.Empty;

    public SubscriptionPlanDto Plan { get; set; } = null!;

    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Active, GracePeriod, or Expired.</summary>
    public string Status { get; set; } = string.Empty;

    public int GracePeriodDays { get; set; }

    public bool IsStorefrontAccessible { get; set; }

    public int? DaysRemaining { get; set; }
}
