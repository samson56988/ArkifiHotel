using Admin.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Seeding;

/// <summary>
/// Ensures restaurant menu rows use a valid branch <see cref="Admin.Data.Entities.BusinessLocation"/> id.
/// Safe to run on every startup (idempotent).
/// </summary>
public sealed class RestaurantMenuLocationBackfillService
{
    private readonly AdminDbContext _db;
    private readonly ILogger<RestaurantMenuLocationBackfillService> _logger;

    public RestaurantMenuLocationBackfillService(
        AdminDbContext db,
        ILogger<RestaurantMenuLocationBackfillService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task BackfillAsync(CancellationToken cancellationToken = default)
    {
        var primaryLocationByBusiness = await _db.BusinessLocations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .GroupBy(l => l.BusinessRegistrationId)
            .Select(g => new { BusinessId = g.Key, LocationId = g.First().Id })
            .ToDictionaryAsync(x => x.BusinessId, x => x.LocationId, cancellationToken)
            .ConfigureAwait(false);

        if (primaryLocationByBusiness.Count == 0)
        {
            return;
        }

        var validLocationIds = primaryLocationByBusiness.Values.ToHashSet();
        var now = DateTimeOffset.UtcNow;
        var updated = 0;

        var settings = await _db.RestaurantMenuSettings
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var row in settings)
        {
            if (validLocationIds.Contains(row.LocationId)
                && await LocationBelongsToBusinessAsync(row.LocationId, row.BusinessRegistrationId, cancellationToken)
                    .ConfigureAwait(false))
            {
                continue;
            }

            if (!primaryLocationByBusiness.TryGetValue(row.BusinessRegistrationId, out var locationId))
            {
                continue;
            }

            row.LocationId = locationId;
            row.UpdatedAt = now;
            updated++;
        }

        var categories = await _db.RestaurantMenuCategories
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var row in categories)
        {
            if (validLocationIds.Contains(row.LocationId)
                && await LocationBelongsToBusinessAsync(row.LocationId, row.BusinessRegistrationId, cancellationToken)
                    .ConfigureAwait(false))
            {
                continue;
            }

            if (!primaryLocationByBusiness.TryGetValue(row.BusinessRegistrationId, out var locationId))
            {
                continue;
            }

            row.LocationId = locationId;
            row.UpdatedAt = now;
            updated++;
        }

        if (updated == 0)
        {
            return;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Backfilled LocationId on {Count} restaurant menu row(s).", updated);
    }

    private Task<bool> LocationBelongsToBusinessAsync(
        Guid locationId,
        Guid businessId,
        CancellationToken cancellationToken) =>
        _db.BusinessLocations
            .AsNoTracking()
            .AnyAsync(l => l.Id == locationId && l.BusinessRegistrationId == businessId, cancellationToken);
}
