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
        var amenities = await _db.Amenities
            .AsNoTracking()
            .Where(a => a.BusinessRegistrationId == businessId)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return amenities.Select(Map).ToList();
    }

    public async Task<AmenityDto?> GetAsync(
        Guid businessId,
        Guid amenityId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Amenities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == amenityId && a.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : Map(entity);
    }

    public async Task<AmenityDto?> CreateAsync(
        Guid businessId,
        CreateCustomAmenityRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length < 2 || name.Length > 128)
        {
            return null;
        }

        var category = NormalizeCategory(request.Category);
        if (await HasDuplicateNameAsync(businessId, name, excludeAmenityId: null, cancellationToken).ConfigureAwait(false))
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

    public async Task<AmenityDto?> UpdateAsync(
        Guid businessId,
        Guid amenityId,
        UpdateAmenityRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length < 2 || name.Length > 128)
        {
            return null;
        }

        var category = NormalizeCategory(request.Category);

        var entity = await _db.Amenities
            .FirstOrDefaultAsync(
                a => a.Id == amenityId && a.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        if (await HasDuplicateNameAsync(businessId, name, amenityId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        entity.Name = name;
        entity.Category = category;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(entity);
    }

    public async Task<AmenityDeleteOutcome> DeleteAsync(
        Guid businessId,
        Guid amenityId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.Amenities
            .FirstOrDefaultAsync(
                a => a.Id == amenityId && a.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return AmenityDeleteOutcome.NotFound;
        }

        var inUse = await _db.RoomAmenities
            .AsNoTracking()
            .AnyAsync(ra => ra.AmenityId == amenityId, cancellationToken)
            .ConfigureAwait(false);

        if (inUse)
        {
            return AmenityDeleteOutcome.InUseByRooms;
        }

        _db.Amenities.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return AmenityDeleteOutcome.Deleted;
    }

    private async Task<bool> HasDuplicateNameAsync(
        Guid businessId,
        string name,
        Guid? excludeAmenityId,
        CancellationToken cancellationToken)
    {
        var nameLower = name.ToLowerInvariant();
        var query = _db.Amenities
            .AsNoTracking()
            .Where(a => a.BusinessRegistrationId == businessId && a.Name.ToLower() == nameLower);

        if (excludeAmenityId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAmenityId.Value);
        }

        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string? NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return null;
        }

        var trimmed = category.Trim();
        return trimmed.Length > 64 ? trimmed[..64] : trimmed;
    }

    private static AmenityDto Map(Amenity a) =>
        new()
        {
            Id = a.Id,
            Name = a.Name,
            Category = a.Category,
            IsCustom = true,
        };
}
