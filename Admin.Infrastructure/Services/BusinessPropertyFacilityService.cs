using Admin.Data;
using Admin.Data.Entities;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessPropertyFacilityService : IBusinessPropertyFacilityService
{
    private readonly AdminDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IOrganizationUserContext _actor;

    public BusinessPropertyFacilityService(
        AdminDbContext db,
        IWebHostEnvironment env,
        IOrganizationUserContext actor)
    {
        _db = db;
        _env = env;
        _actor = actor;
    }

    public async Task<IReadOnlyList<PropertyFacilitySummaryDto>> ListAsync(
        Guid businessId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.PropertyFacilities
            .AsNoTracking()
            .Include(f => f.Images)
            .Include(f => f.Location)
            .Where(f => f.BusinessRegistrationId == businessId);

        query = OrganizationQueryScope.ApplyFacilityScope(query, _actor);

        if (!includeArchived)
        {
            query = query.Where(f => !f.IsArchived);
        }

        var rows = await query
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(f => new PropertyFacilitySummaryDto
            {
                Id = f.Id,
                Name = f.Name,
                PrimaryImageUrl = f.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
                    .FirstOrDefault(),
                ImageCount = f.Images.Count,
                LocationId = f.LocationId,
                LocationName = f.Location?.Name,
                IsArchived = f.IsArchived,
            })
            .ToList();
    }

    public async Task<PropertyFacilityDetailDto?> GetAsync(
        Guid businessId,
        Guid facilityId,
        CancellationToken cancellationToken = default)
    {
        var f = await _db.PropertyFacilities
            .AsNoTracking()
            .Include(x => x.Images)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(
                x => x.Id == facilityId && x.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        return f is null ? null : MapDetail(f);
    }

    public async Task<PropertyFacilityDetailDto?> CreateAsync(
        Guid businessId,
        CreatePropertyFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateName(request.Name, out var name) || !ValidateDescription(request.Description))
        {
            return null;
        }

        if (!await BusinessExistsAsync(businessId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        if (!await LocationAllowedForBusinessAsync(businessId, request.LocationId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PropertyFacility
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            LocationId = request.LocationId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = now,
        };

        _db.PropertyFacilities.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, entity.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PropertyFacilityDetailDto?> UpdateAsync(
        Guid businessId,
        Guid facilityId,
        UpdatePropertyFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateName(request.Name, out var name) || !ValidateDescription(request.Description))
        {
            return null;
        }

        var entity = await _db.PropertyFacilities
            .FirstOrDefaultAsync(f => f.Id == facilityId && f.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        if (!await LocationAllowedForBusinessAsync(businessId, request.LocationId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.LocationId = request.LocationId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, entity.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(Guid businessId, Guid facilityId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.PropertyFacilities
            .Include(f => f.Images)
            .FirstOrDefaultAsync(f => f.Id == facilityId && f.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        foreach (var img in entity.Images)
        {
            TryDeletePhysicalFile(img.RelativePath);
        }

        _db.PropertyFacilities.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> SetArchivedAsync(
        Guid businessId,
        Guid facilityId,
        bool archived,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.PropertyFacilities
            .FirstOrDefaultAsync(f => f.Id == facilityId && f.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        entity.IsArchived = archived;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<FacilityImageDto?> AddImageAsync(
        Guid businessId,
        Guid facilityId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRelativePath(relativePathUnderWwwroot);
        if (normalized is null)
        {
            return null;
        }

        var facility = await _db.PropertyFacilities
            .FirstOrDefaultAsync(f => f.Id == facilityId && f.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (facility is null)
        {
            return null;
        }

        var maxOrder = await _db.PropertyFacilityImages
            .Where(i => i.PropertyFacilityId == facilityId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? -1;

        var entity = new PropertyFacilityImage
        {
            Id = Guid.NewGuid(),
            PropertyFacilityId = facilityId,
            RelativePath = normalized,
            OriginalFileName = SanitizeOriginalFileName(originalFileName),
            SortOrder = maxOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.PropertyFacilityImages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapImage(entity);
    }

    public async Task<bool> DeleteImageAsync(
        Guid businessId,
        Guid facilityId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var image = await _db.PropertyFacilityImages
            .Include(i => i.PropertyFacility)
            .FirstOrDefaultAsync(
                i => i.Id == imageId
                    && i.PropertyFacilityId == facilityId
                    && i.PropertyFacility.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (image is null)
        {
            return false;
        }

        TryDeletePhysicalFile(image.RelativePath);
        _db.PropertyFacilityImages.Remove(image);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> BusinessExistsAsync(Guid businessId, CancellationToken cancellationToken) =>
        await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

    private async Task<bool> LocationAllowedForBusinessAsync(
        Guid businessId,
        Guid? locationId,
        CancellationToken cancellationToken)
    {
        if (!locationId.HasValue || locationId.Value == Guid.Empty)
        {
            return false;
        }

        return await _db.BusinessLocations
            .AsNoTracking()
            .AnyAsync(l => l.Id == locationId.Value && l.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);
    }

    private static PropertyFacilityDetailDto MapDetail(PropertyFacility f)
    {
        var images = f.Images
            .OrderBy(i => i.SortOrder)
            .Select(MapImage)
            .ToList();

        return new PropertyFacilityDetailDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            LocationId = f.LocationId,
            LocationName = f.Location?.Name,
            Images = images,
            IsArchived = f.IsArchived,
        };
    }

    private static FacilityImageDto MapImage(PropertyFacilityImage i) =>
        new()
        {
            Id = i.Id,
            Url = "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal),
            OriginalFileName = i.OriginalFileName,
            SortOrder = i.SortOrder,
        };

    private static bool ValidateName(string? name, out string trimmed)
    {
        trimmed = name?.Trim() ?? string.Empty;
        return trimmed.Length is >= 2 and <= 200;
    }

    private static bool ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return true;
        }

        return description.Trim().Length <= 4000;
    }

    private void TryDeletePhysicalFile(string relativePath)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var combined = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        string full;
        try
        {
            full = Path.GetFullPath(combined);
        }
        catch
        {
            return;
        }

        var root = Path.GetFullPath(webRoot);
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            if (File.Exists(full))
            {
                File.Delete(full);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static string? SanitizeOriginalFileName(string? originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return null;
        }

        var t = originalFileName.Trim();
        return t.Length > 255 ? t[..255] : t;
    }

    private static string? NormalizeRelativePath(string path)
    {
        var p = path.Trim().Replace('\\', '/');
        if (p.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
        {
            p = p["wwwroot/".Length..];
        }

        if (p.StartsWith('/'))
        {
            p = p[1..];
        }

        if (!p.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (p.Contains("..", StringComparison.Ordinal))
        {
            return null;
        }

        return p;
    }
}
