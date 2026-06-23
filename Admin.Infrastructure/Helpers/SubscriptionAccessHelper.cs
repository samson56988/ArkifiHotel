using Admin.Data.Entities;

namespace Admin.Infrastructure.Helpers;

public static class SubscriptionAccessHelper
{
    /// <summary>Days after expiry before the guest storefront slug is blocked.</summary>
    public const int StorefrontGracePeriodDays = 2;

    /// <summary>Initial trial length for new and migrated businesses.</summary>
    public const int TrialDays = 30;

    public static bool IsStorefrontAccessible(BusinessRegistration business, DateTimeOffset? atUtc = null)
    {
        var now = atUtc ?? DateTimeOffset.UtcNow;
        if (!business.SubscriptionExpiresAt.HasValue)
        {
            return true;
        }

        var blockAfter = business.SubscriptionExpiresAt.Value.AddDays(StorefrontGracePeriodDays);
        return now <= blockAfter;
    }

    public static SubscriptionAccessStatus GetAccessStatus(BusinessRegistration business, DateTimeOffset? atUtc = null)
    {
        var now = atUtc ?? DateTimeOffset.UtcNow;
        if (!business.SubscriptionExpiresAt.HasValue)
        {
            return SubscriptionAccessStatus.Active;
        }

        if (now <= business.SubscriptionExpiresAt.Value)
        {
            return SubscriptionAccessStatus.Active;
        }

        var blockAfter = business.SubscriptionExpiresAt.Value.AddDays(StorefrontGracePeriodDays);
        if (now <= blockAfter)
        {
            return SubscriptionAccessStatus.GracePeriod;
        }

        return SubscriptionAccessStatus.Expired;
    }

    public static DateTimeOffset ComputeTrialExpiry(DateTimeOffset fromUtc) =>
        fromUtc.AddDays(TrialDays);

    public static DateTimeOffset ComputeProExpiry(DateTimeOffset fromUtc, Admin.Data.Enums.SubscriptionBillingInterval interval) =>
        interval switch
        {
            Admin.Data.Enums.SubscriptionBillingInterval.Yearly => fromUtc.AddYears(1),
            Admin.Data.Enums.SubscriptionBillingInterval.Monthly => fromUtc.AddMonths(1),
            _ => ComputeTrialExpiry(fromUtc),
        };
}

public enum SubscriptionAccessStatus
{
    Active,
    GracePeriod,
    Expired,
}
