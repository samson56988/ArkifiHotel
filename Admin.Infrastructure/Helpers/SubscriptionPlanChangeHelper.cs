using Admin.Data.Entities;
using Admin.Data.Enums;

namespace Admin.Infrastructure.Helpers;

public enum SubscriptionPlanChangeType
{
    Current,
    Upgrade,
    Downgrade,
    Renew,
}

public readonly record struct SubscriptionPlanChangeEvaluation(
    SubscriptionPlanChangeType ChangeType,
    bool Allowed,
    bool RequiresPayment,
    string? DisabledReason)
{
    public static SubscriptionPlanChangeEvaluation Current(string? reason = null) =>
        new(SubscriptionPlanChangeType.Current, false, false, reason ?? "This is your current plan.");

    public static SubscriptionPlanChangeEvaluation Ok(
        SubscriptionPlanChangeType type,
        bool requiresPayment) =>
        new(type, true, requiresPayment, null);

    public static SubscriptionPlanChangeEvaluation Denied(SubscriptionPlanChangeType type, string reason) =>
        new(type, false, false, reason);
}

public static class SubscriptionPlanChangeHelper
{
    public static int GetRank(SubscriptionPlan plan)
    {
        if (plan.Tier == SubscriptionPlanTier.Free)
        {
            return 0;
        }

        return plan.BillingInterval switch
        {
            SubscriptionBillingInterval.Yearly => 2,
            SubscriptionBillingInterval.Monthly => 1,
            _ => 1,
        };
    }

    public static bool IsDowngrade(SubscriptionPlan current, SubscriptionPlan target) =>
        GetRank(target) < GetRank(current);

    public static bool IsUpgrade(SubscriptionPlan current, SubscriptionPlan target) =>
        GetRank(target) > GetRank(current);

    public static bool HasExpired(DateTimeOffset? expiresAt, DateTimeOffset now) =>
        expiresAt.HasValue && now > expiresAt.Value;

    public static SubscriptionPlanChangeEvaluation Evaluate(
        BusinessRegistration business,
        SubscriptionPlan current,
        SubscriptionPlan target,
        DateTimeOffset now)
    {
        if (current.Id == target.Id)
        {
            if (current.Tier != SubscriptionPlanTier.Free && HasExpired(business.SubscriptionExpiresAt, now))
            {
                return SubscriptionPlanChangeEvaluation.Ok(SubscriptionPlanChangeType.Renew, requiresPayment: true);
            }

            return SubscriptionPlanChangeEvaluation.Current();
        }

        if (IsDowngrade(current, target))
        {
            if (!HasExpired(business.SubscriptionExpiresAt, now))
            {
                return SubscriptionPlanChangeEvaluation.Denied(
                    SubscriptionPlanChangeType.Downgrade,
                    "You can only downgrade after your current plan expires.");
            }

            return SubscriptionPlanChangeEvaluation.Ok(
                SubscriptionPlanChangeType.Downgrade,
                requiresPayment: target.Tier != SubscriptionPlanTier.Free && target.PriceAmount > 0);
        }

        if (IsUpgrade(current, target))
        {
            return SubscriptionPlanChangeEvaluation.Ok(
                SubscriptionPlanChangeType.Upgrade,
                requiresPayment: target.PriceAmount > 0);
        }

        return SubscriptionPlanChangeEvaluation.Denied(
            SubscriptionPlanChangeType.Current,
            "This plan change is not available.");
    }

    public static DateTimeOffset ComputeNewExpiry(
        DateTimeOffset now,
        SubscriptionPlan target,
        SubscriptionPlanChangeType changeType,
        DateTimeOffset? currentExpiresAt)
    {
        if (target.Tier == SubscriptionPlanTier.Free)
        {
            return currentExpiresAt ?? now;
        }

        if (changeType == SubscriptionPlanChangeType.Renew
            && currentExpiresAt.HasValue
            && currentExpiresAt.Value > now)
        {
            return SubscriptionAccessHelper.ComputeProExpiry(currentExpiresAt.Value, target.BillingInterval);
        }

        return SubscriptionAccessHelper.ComputeProExpiry(now, target.BillingInterval);
    }
}
