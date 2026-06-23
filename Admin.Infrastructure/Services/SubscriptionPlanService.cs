using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class SubscriptionPlanService : ISubscriptionPlanService
{
    private readonly AdminDbContext _db;

    public SubscriptionPlanService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return plans.Select(Map).ToList();
    }

    internal static SubscriptionPlanDto Map(SubscriptionPlan plan) =>
        new()
        {
            Id = plan.Id,
            Code = plan.Code,
            Name = plan.Name,
            Description = plan.Description,
            Tier = plan.Tier == SubscriptionPlanTier.Pro ? "Pro" : "Free",
            BillingInterval = plan.BillingInterval switch
            {
                SubscriptionBillingInterval.Monthly => "Monthly",
                SubscriptionBillingInterval.Yearly => "Yearly",
                _ => "None",
            },
            PriceAmount = plan.PriceAmount,
            Currency = plan.Currency,
            YearlyDiscountPercent = plan.YearlyDiscountPercent,
            SortOrder = plan.SortOrder,
        };
}
