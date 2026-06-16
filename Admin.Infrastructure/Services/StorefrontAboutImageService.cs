using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class StorefrontAboutImageService : IStorefrontAboutImageService
{
    private readonly AdminDbContext _db;

    public StorefrontAboutImageService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<StorefrontAboutImageDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.StorefrontAboutImages
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : Map(entity);
    }

    public async Task<StorefrontAboutImageDto?> UpsertAsync(
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

        var entity = await _db.StorefrontAboutImages
            .FirstOrDefaultAsync(i => i.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            entity = new StorefrontAboutImage
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = businessId,
            };
            _db.StorefrontAboutImages.Add(entity);
        }

        entity.RelativePath = normalized;
        entity.OriginalFileName = SanitizeOriginalFileName(originalFileName);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.StorefrontAboutImages
            .FirstOrDefaultAsync(i => i.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        _db.StorefrontAboutImages.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static StorefrontAboutImageDto Map(StorefrontAboutImage entity) =>
        new()
        {
            Id = entity.Id,
            Url = "/" + entity.RelativePath.Replace("\\", "/", StringComparison.Ordinal),
            OriginalFileName = entity.OriginalFileName,
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
