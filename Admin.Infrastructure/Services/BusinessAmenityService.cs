using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessAmenityService : IBusinessAmenityService
{
    private readonly AdminDbContext _db;

    public BusinessAmenityService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AmenityDto>> ListForBusinessAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var catalog = await _db.Amenities
            .AsNoTracking()
            .Where(a => a.BusinessRegistrationId == null)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var custom = await _db.Amenities
            .AsNoTracking()
            .Where(a => a.BusinessRegistrationId == businessId)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return catalog.Select(Map).Concat(custom.Select(Map)).ToList();
    }

    public async Task<AmenityDto?> CreateCustomAsync(
        Guid businessId,
        CreateCustomAmenityRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length < 2 || name.Length > 128)
        {
            return null;
        }

        var category = string.IsNullOrWhiteSpace(request.Category)
            ? null
            : request.Category.Trim();

        if (category is { Length: > 64 })
        {
            category = category[..64];
        }

        var nameLower = name.ToLowerInvariant();
        var duplicate = await _db.Amenities
            .AsNoTracking()
            .AnyAsync(
                a => a.BusinessRegistrationId == businessId
                    && a.Name.ToLower() == nameLower,
                cancellationToken)
            .ConfigureAwait(false);

        if (duplicate)
        {
            return null;
        }

        var conflictsCatalog = await _db.Amenities
            .AsNoTracking()
            .AnyAsync(
                a => a.BusinessRegistrationId == null && a.Name.ToLower() == nameLower,
                cancellationToken)
            .ConfigureAwait(false);

        if (conflictsCatalog)
        {
            return null;
        }

        var entity = new Amenity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            BusinessRegistrationId = businessId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Amenities.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(entity);
    }

    private static AmenityDto Map(Amenity a) =>
        new()
        {
            Id = a.Id,
            Name = a.Name,
            Category = a.Category,
            IsCustom = a.BusinessRegistrationId.HasValue,
        };
}
