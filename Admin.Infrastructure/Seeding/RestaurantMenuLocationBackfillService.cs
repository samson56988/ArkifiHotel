using Admin.Data;
using Admin.Data.Entities;
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
        var locations = await _db.BusinessLocations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (locations.Count == 0)
        {
            return;
        }

        var primaryLocationByBusiness = locations
            .GroupBy(l => l.BusinessRegistrationId)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var locationBusinessMap = locations.ToDictionary(l => l.Id, l => l.BusinessRegistrationId);
        var now = DateTimeOffset.UtcNow;
        var updated = 0;
        var removed = 0;

        var settings = await _db.RestaurantMenuSettings
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var settingsToRemove = new List<RestaurantMenuSettings>();
        var occupiedSettingsKeys = new HashSet<(Guid BusinessId, Guid LocationId)>();

        foreach (var row in settings.OrderBy(s => s.CreatedAt))
        {
            var hasValidLocation = locationBusinessMap.TryGetValue(row.LocationId, out var ownerBusinessId)
                && ownerBusinessId == row.BusinessRegistrationId;

            if (hasValidLocation)
            {
                var key = (row.BusinessRegistrationId, row.LocationId);
                if (!occupiedSettingsKeys.Add(key))
                {
                    settingsToRemove.Add(row);
                }

                continue;
            }

            if (!primaryLocationByBusiness.TryGetValue(row.BusinessRegistrationId, out var locationId))
            {
                continue;
            }

            var targetKey = (row.BusinessRegistrationId, locationId);
            if (occupiedSettingsKeys.Contains(targetKey))
            {
                settingsToRemove.Add(row);
                continue;
            }

            row.LocationId = locationId;
            row.UpdatedAt = now;
            occupiedSettingsKeys.Add(targetKey);
            updated++;
        }

        if (settingsToRemove.Count > 0)
        {
            _db.RestaurantMenuSettings.RemoveRange(settingsToRemove);
            removed = settingsToRemove.Count;
        }

        var categories = await _db.RestaurantMenuCategories
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var row in categories)
        {
            var hasValidLocation = locationBusinessMap.TryGetValue(row.LocationId, out var ownerBusinessId)
                && ownerBusinessId == row.BusinessRegistrationId;

            if (hasValidLocation)
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

        if (updated == 0 && removed == 0)
        {
            return;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation(
            "Restaurant menu location backfill: updated {Updated} row(s), removed {Removed} duplicate settings row(s).",
            updated,
            removed);
    }
}
