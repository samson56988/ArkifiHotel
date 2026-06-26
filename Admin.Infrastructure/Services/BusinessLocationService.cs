using Admin.Data;
using Admin.Data.Entities;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessLocationService : IBusinessLocationService
{
    private readonly AdminDbContext _db;
    private readonly IOrganizationUserContext _actor;

    public BusinessLocationService(AdminDbContext db, IOrganizationUserContext actor)
    {
        _db = db;
        _actor = actor;
    }

    public async Task<IReadOnlyList<BusinessLocationDto>> ListAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var query = _db.BusinessLocations
            .AsNoTracking()
            .Where(l => l.BusinessRegistrationId == businessId);

        query = OrganizationQueryScope.ApplyLocationScope(query, _actor);

        var rows = await query
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(Map).ToList();
    }

    public async Task<BusinessLocationDto?> GetAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.Id == locationId && l.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : Map(entity);
    }

    public async Task<BusinessLocationDto?> CreateAsync(
        Guid businessId,
        CreateBusinessLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateCore(request.Name, request.Address, out var name, out var address))
        {
            return null;
        }

        if (!await BusinessExistsAsync(businessId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        if (await HasDuplicateNameAsync(businessId, name, excludeLocationId: null, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new BusinessLocation
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            Name = name,
            Address = address,
            CreatedAt = now,
        };

        _db.BusinessLocations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(entity);
    }

    public async Task<BusinessLocationDto?> UpdateAsync(
        Guid businessId,
        Guid locationId,
        UpdateBusinessLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateCore(request.Name, request.Address, out var name, out var address))
        {
            return null;
        }

        var entity = await _db.BusinessLocations
            .FirstOrDefaultAsync(
                l => l.Id == locationId && l.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        if (await HasDuplicateNameAsync(businessId, name, locationId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        entity.Name = name;
        entity.Address = address;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(entity);
    }

    public async Task<LocationDeleteOutcome> DeleteAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessLocations
            .FirstOrDefaultAsync(
                l => l.Id == locationId && l.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return LocationDeleteOutcome.NotFound;
        }

        var inUse = await _db.Rooms
            .AsNoTracking()
            .AnyAsync(r => r.LocationId == locationId, cancellationToken)
            .ConfigureAwait(false);

        if (!inUse)
        {
            inUse = await _db.PropertyFacilities
                .AsNoTracking()
                .AnyAsync(f => f.LocationId == locationId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (!inUse)
        {
            inUse = await _db.StorefrontBannerImages
                .AsNoTracking()
                .AnyAsync(i => i.LocationId == locationId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (inUse)
        {
            return LocationDeleteOutcome.InUse;
        }

        _db.BusinessLocations.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return LocationDeleteOutcome.Deleted;
    }

    private async Task<bool> BusinessExistsAsync(Guid businessId, CancellationToken cancellationToken) =>
        await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

    private async Task<bool> HasDuplicateNameAsync(
        Guid businessId,
        string name,
        Guid? excludeLocationId,
        CancellationToken cancellationToken)
    {
        var nameLower = name.ToLowerInvariant();
        var query = _db.BusinessLocations
            .AsNoTracking()
            .Where(l => l.BusinessRegistrationId == businessId && l.Name.ToLower() == nameLower);

        if (excludeLocationId.HasValue)
        {
            query = query.Where(l => l.Id != excludeLocationId.Value);
        }

        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool ValidateCore(
        string? nameInput,
        string? addressInput,
        out string name,
        out string? address)
    {
        name = nameInput?.Trim() ?? string.Empty;
        if (name.Length < 2 || name.Length > 200)
        {
            address = null;
            return false;
        }

        if (string.IsNullOrWhiteSpace(addressInput))
        {
            address = null;
            return true;
        }

        var trimmed = addressInput.Trim();
        address = trimmed.Length > 500 ? trimmed[..500] : trimmed;
        return true;
    }

    private static BusinessLocationDto Map(BusinessLocation l) =>
        new()
        {
            Id = l.Id,
            Name = l.Name,
            Address = l.Address,
            CreatedAt = l.CreatedAt,
        };
}
