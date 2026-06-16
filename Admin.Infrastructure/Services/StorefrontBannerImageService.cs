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
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.StorefrontBannerImages
            .AsNoTracking()
            .Where(i => i.BusinessRegistrationId == businessId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(Map).ToList();
    }

    public async Task<int> CountAsync(Guid businessId, CancellationToken cancellationToken = default) =>
        await _db.StorefrontBannerImages
            .CountAsync(i => i.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<StorefrontBannerImageDto?> AddImageAsync(
        Guid businessId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRelativePath(relativePathUnderWwwroot);
        if (normalized is null)
        {
            return null;
        }

        var exists = await _db.BusinessRegistrations
            .AnyAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            return null;
        }

        var count = await CountAsync(businessId, cancellationToken).ConfigureAwait(false);
        if (count >= IStorefrontBannerImageService.MaxImages)
        {
            return null;
        }

        var maxOrder = await _db.StorefrontBannerImages
            .Where(i => i.BusinessRegistrationId == businessId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? -1;

        var entity = new StorefrontBannerImage
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            RelativePath = normalized,
            OriginalFileName = SanitizeOriginalFileName(originalFileName),
            SortOrder = maxOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.StorefrontBannerImages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
