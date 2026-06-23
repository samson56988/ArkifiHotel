using Admin.Data;
using Admin.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Seeding;

/// <summary>Backfills subscription plan and trial expiry for businesses created before subscriptions existed.</summary>
public sealed class BusinessSubscriptionSeedService
{
    private readonly AdminDbContext _db;
    private readonly ILogger<BusinessSubscriptionSeedService> _logger;

    public BusinessSubscriptionSeedService(AdminDbContext db, ILogger<BusinessSubscriptionSeedService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedMissingSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var freePlanExists = await _db.SubscriptionPlans
            .AnyAsync(p => p.Id == SubscriptionPlanSeed.FreePlanId, cancellationToken)
            .ConfigureAwait(false);

        if (!freePlanExists)
        {
            _logger.LogWarning("Subscription plans are not seeded yet; skipping business subscription backfill.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var trialExpiry = Admin.Infrastructure.Helpers.SubscriptionAccessHelper.ComputeTrialExpiry(now);

        var businesses = await _db.BusinessRegistrations
            .Where(b => b.SubscriptionPlanId == Guid.Empty || b.SubscriptionExpiresAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (businesses.Count == 0)
        {
            return;
        }

        foreach (var business in businesses)
        {
            if (business.SubscriptionPlanId == Guid.Empty)
            {
                business.SubscriptionPlanId = SubscriptionPlanSeed.FreePlanId;
            }

            business.SubscriptionExpiresAt ??= trialExpiry;
            business.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Backfilled subscription for {Count} business(es).", businesses.Count);
    }
}
