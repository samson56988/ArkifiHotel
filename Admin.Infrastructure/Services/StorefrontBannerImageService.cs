using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class StorefrontBannerImageService : IStorefrontBannerImageService
{
    private readonly AdminDbContext _db;

    public StorefrontBannerImageService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<StorefrontBannerImageDto>> GetAsync(
        Guid businessId,
        Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.StorefrontBannerImages
            .AsNoTracking()
            .Include(i => i.Location)
            .Where(i => i.BusinessRegistrationId == businessId);

        if (locationId.HasValue)
        {
            query = query.Where(i => i.LocationId == locationId.Value);
        }

        var rows = await query
            .OrderBy(i => i.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(Map).ToList();
    }

    public async Task<int> CountAsync(Guid businessId, Guid locationId, CancellationToken cancellationToken = default) =>
        await _db.StorefrontBannerImages
            .CountAsync(
                i => i.BusinessRegistrationId == businessId && i.LocationId == locationId,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<StorefrontBannerImageDto?> AddImageAsync(
        Guid businessId,
        Guid locationId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRelativePath(relativePathUnderWwwroot);
        if (normalized is null)
        {
            return null;
        }

        var location = await _db.BusinessLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.Id == locationId && l.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (location is null)
        {
            return null;
        }

        var count = await CountAsync(businessId, locationId, cancellationToken).ConfigureAwait(false);
        if (count >= IStorefrontBannerImageService.MaxImages)
        {
            return null;
        }

        var maxOrder = await _db.StorefrontBannerImages
            .Where(i => i.BusinessRegistrationId == businessId && i.LocationId == locationId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? -1;

        var entity = new StorefrontBannerImage
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            LocationId = locationId,
            RelativePath = normalized,
            OriginalFileName = SanitizeOriginalFileName(originalFileName),
            SortOrder = maxOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.StorefrontBannerImages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        entity.Location = location;
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid businessId, Guid imageId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.StorefrontBannerImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        _db.StorefrontBannerImages.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static StorefrontBannerImageDto Map(StorefrontBannerImage entity) =>
        new()
        {
            Id = entity.Id,
            Url = "/" + entity.RelativePath.Replace("\\", "/", StringComparison.Ordinal),
            OriginalFileName = entity.OriginalFileName,
            SortOrder = entity.SortOrder,
            LocationId = entity.LocationId,
            LocationName = entity.Location?.Name,
        };

    private static string? NormalizeRelativePath(string path)
    {
        var trimmed = path?.Trim().Replace('\\', '/');
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return trimmed.TrimStart('/');
    }

    private static string? SanitizeOriginalFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmed = name.Trim();
        return trimmed.Length > 256 ? trimmed[..256] : trimmed;
    }
}
