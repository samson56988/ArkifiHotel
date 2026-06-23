using Admin.Data.Entities;
using Admin.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Admin.Data.Seeding;

public static class SubscriptionPlanSeed
{
    public static readonly Guid FreePlanId = new("fa000002-0000-4000-8000-000000000001");
    public static readonly Guid ProMonthlyPlanId = new("fa000002-0000-4000-8000-000000000002");
    public static readonly Guid ProYearlyPlanId = new("fa000002-0000-4000-8000-000000000003");

    private static readonly DateTimeOffset SeedTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public const int ProYearlyDiscountPercent = 20;

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan
            {
                Id = FreePlanId,
                Code = "free",
                Name = "Free",
                Description = "Try ArkifiStay with a 30-day trial. Perfect for getting started.",
                Tier = SubscriptionPlanTier.Free,
                BillingInterval = SubscriptionBillingInterval.None,
                PriceAmount = 0m,
                Currency = "NGN",
                YearlyDiscountPercent = null,
                IsActive = true,
                SortOrder = 0,
                CreatedAt = SeedTime,
            },
            new SubscriptionPlan
            {
                Id = ProMonthlyPlanId,
                Code = "pro-monthly",
                Name = "Pro",
                Description = "Full platform access with storefront, bookings, restaurant, and payments.",
                Tier = SubscriptionPlanTier.Pro,
                BillingInterval = SubscriptionBillingInterval.Monthly,
                PriceAmount = 20_000m,
                Currency = "NGN",
                YearlyDiscountPercent = null,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = SeedTime,
            },
            new SubscriptionPlan
            {
                Id = ProYearlyPlanId,
                Code = "pro-yearly",
                Name = "Pro (Yearly)",
                Description = "Pro plan billed yearly — save 20% compared to paying monthly.",
                Tier = SubscriptionPlanTier.Pro,
                BillingInterval = SubscriptionBillingInterval.Yearly,
                PriceAmount = 192_000m,
                Currency = "NGN",
                YearlyDiscountPercent = ProYearlyDiscountPercent,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = SeedTime,
            });
    }
}
