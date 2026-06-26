using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessEventHallService : IBusinessEventHallService
{
    private readonly AdminDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IOrganizationUserContext _actor;

    public BusinessEventHallService(AdminDbContext db, IWebHostEnvironment env, IOrganizationUserContext actor)
    {
        _db = db;
        _env = env;
        _actor = actor;
    }

    public async Task<IReadOnlyList<EventHallSummaryDto>> ListAsync(
        Guid businessId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.EventHalls
            .AsNoTracking()
            .Include(h => h.Images)
            .Include(h => h.Location)
            .Where(h => h.BusinessRegistrationId == businessId);

        query = OrganizationQueryScope.ApplyEventHallScope(query, _actor);

        if (!includeArchived)
        {
            query = query.Where(h => !h.IsArchived);
        }

        var rows = await query
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(h => new EventHallSummaryDto
            {
                Id = h.Id,
                Name = h.Name,
                RentalPrice = h.RentalPrice,
                MaxCapacity = h.MaxCapacity,
                PrimaryImageUrl = PrimaryImageUrl(h.Images),
                ImageCount = h.Images.Count,
                LocationId = h.LocationId,
                LocationName = h.Location?.Name,
                IsArchived = h.IsArchived,
            })
            .ToList();
    }

    public async Task<EventHallDetailDto?> GetAsync(
        Guid businessId,
        Guid eventHallId,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.EventHalls
            .AsNoTracking()
            .Include(h => h.Images)
            .Include(h => h.Location)
            .FirstOrDefaultAsync(
                h => h.Id == eventHallId && h.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        return row is null ? null : MapDetail(row);
    }

    public async Task<EventHallDetailDto?> CreateAsync(
        Guid businessId,
        CreateEventHallRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateName(request.Name, out var name) ||
            !ValidateDescription(request.Description) ||
            !ValidateRentalPrice(request.RentalPrice))
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

        var entity = new EventHall
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            LocationId = request.LocationId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            RentalPrice = request.RentalPrice,
            MaxCapacity = request.MaxCapacity,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.EventHalls.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, entity.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<EventHallDetailDto?> UpdateAsync(
        Guid businessId,
        Guid eventHallId,
        UpdateEventHallRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateName(request.Name, out var name) ||
            !ValidateDescription(request.Description) ||
            !ValidateRentalPrice(request.RentalPrice))
        {
            return null;
        }

        var entity = await _db.EventHalls
            .FirstOrDefaultAsync(h => h.Id == eventHallId && h.BusinessRegistrationId == businessId, cancellationToken)
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
        entity.RentalPrice = request.RentalPrice;
        entity.MaxCapacity = request.MaxCapacity;
        entity.LocationId = request.LocationId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, entity.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ArchiveAsync(Guid businessId, Guid eventHallId, CancellationToken cancellationToken = default) =>
        await SetArchivedAsync(businessId, eventHallId, true, cancellationToken).ConfigureAwait(false);

    public async Task<bool> RestoreAsync(Guid businessId, Guid eventHallId, CancellationToken cancellationToken = default) =>
        await SetArchivedAsync(businessId, eventHallId, false, cancellationToken).ConfigureAwait(false);

    public async Task<bool> DeleteAsync(Guid businessId, Guid eventHallId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.EventHalls
            .Include(h => h.Images)
            .FirstOrDefaultAsync(h => h.Id == eventHallId && h.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        foreach (var img in entity.Images)
        {
            TryDeletePhysicalFile(img.RelativePath);
        }

        _db.EventHalls.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<EventHallDetailDto?> AddImageAsync(
        Guid businessId,
        Guid eventHallId,
        string relativePath,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRelativePath(relativePath);
        if (normalized is null)
        {
            return null;
        }

        var hall = await _db.EventHalls
            .FirstOrDefaultAsync(h => h.Id == eventHallId && h.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (hall is null)
        {
            return null;
        }

        var maxOrder = await _db.EventHallImages
            .Where(i => i.EventHallId == eventHallId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? -1;

        var entity = new EventHallImage
        {
            Id = Guid.NewGuid(),
            EventHallId = eventHallId,
            RelativePath = normalized,
            OriginalFileName = SanitizeOriginalFileName(originalFileName),
            SortOrder = maxOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.EventHallImages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, eventHallId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> RemoveImageAsync(
        Guid businessId,
        Guid eventHallId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var image = await _db.EventHallImages
            .Include(i => i.EventHall)
            .FirstOrDefaultAsync(
                i => i.Id == imageId
                    && i.EventHallId == eventHallId
                    && i.EventHall.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (image is null)
        {
            return false;
        }

        TryDeletePhysicalFile(image.RelativePath);
        _db.EventHallImages.Remove(image);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<EventHallRequestListItemDto>> ListRequestsAsync(
        Guid businessId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _db.EventHallRequests
            .AsNoTracking()
            .Include(r => r.EventHall)
            .Include(r => r.Location)
            .Where(r => r.BusinessRegistrationId == businessId);

        if (TryParseStatus(status, out var parsed))
        {
            query = query.Where(r => r.Status == parsed);
        }

        var rows = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(r => new EventHallRequestListItemDto
            {
                Id = r.Id,
                EventHallName = r.EventHall.Name,
                GuestName = r.GuestName,
                GuestEmail = r.GuestEmail,
                GuestPhone = r.GuestPhone,
                EventDate = r.EventDate,
                EventEndDate = r.EventEndDate,
                EventPurpose = r.EventPurpose,
                Status = StatusLabel(r.Status),
                LocationName = r.Location?.Name,
                CreatedAt = r.CreatedAt,
            })
            .ToList();
    }

    public async Task<EventHallRequestDetailDto?> GetRequestAsync(
        Guid businessId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.EventHallRequests
            .AsNoTracking()
            .Include(r => r.EventHall)
            .Include(r => r.Location)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return row is null ? null : MapRequestDetail(row);
    }

    public async Task<EventHallRequestDetailDto?> UpdateRequestStatusAsync(
        Guid businessId,
        Guid requestId,
        UpdateEventHallRequestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseStatusRequired(request.Status, out var status))
        {
            return null;
        }

        var row = await _db.EventHallRequests
            .Include(r => r.EventHall)
            .Include(r => r.Location)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
        {
            return null;
        }

        row.Status = status;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapRequestDetail(row);
    }

    public async Task<IReadOnlyList<PublicStorefrontEventHallDto>> GetPublicForLocationAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.EventHalls
            .AsNoTracking()
            .Include(h => h.Images)
            .Where(h => h.BusinessRegistrationId == businessId && h.LocationId == locationId && !h.IsArchived)
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(h =>
            {
                var imageUrls = h.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
                    .ToList();

                return new PublicStorefrontEventHallDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description ?? string.Empty,
                    RentalPrice = h.RentalPrice,
                    MaxCapacity = h.MaxCapacity,
                    PrimaryImageUrl = imageUrls.FirstOrDefault(),
                    ImageUrls = imageUrls,
                };
            })
            .ToList();
    }

    private async Task<bool> SetArchivedAsync(
        Guid businessId,
        Guid eventHallId,
        bool archived,
        CancellationToken cancellationToken)
    {
        var entity = await _db.EventHalls
            .FirstOrDefaultAsync(h => h.Id == eventHallId && h.BusinessRegistrationId == businessId, cancellationToken)
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

    private async Task<bool> BusinessExistsAsync(Guid businessId, CancellationToken cancellationToken) =>
        await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

    private async Task<bool> LocationAllowedForBusinessAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        if (locationId == Guid.Empty)
        {
            return false;
        }

        return await _db.BusinessLocations
            .AsNoTracking()
            .AnyAsync(l => l.Id == locationId && l.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);
    }

    private static EventHallDetailDto MapDetail(EventHall h)
    {
        var images = h.Images
            .OrderBy(i => i.SortOrder)
            .Select(MapImage)
            .ToList();

        return new EventHallDetailDto
        {
            Id = h.Id,
            Name = h.Name,
            Description = h.Description,
            RentalPrice = h.RentalPrice,
            MaxCapacity = h.MaxCapacity,
            LocationId = h.LocationId,
            LocationName = h.Location?.Name,
            IsArchived = h.IsArchived,
            Images = images,
        };
    }

    private static EventHallImageDto MapImage(EventHallImage i) =>
        new()
        {
            Id = i.Id,
            Url = "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal),
            OriginalFileName = i.OriginalFileName,
            SortOrder = i.SortOrder,
        };

    private static EventHallRequestDetailDto MapRequestDetail(EventHallRequest r) =>
        new()
        {
            Id = r.Id,
            EventHallId = r.EventHallId,
            EventHallName = r.EventHall.Name,
            GuestName = r.GuestName,
            GuestEmail = r.GuestEmail,
            GuestPhone = r.GuestPhone,
            EventDate = r.EventDate,
            EventEndDate = r.EventEndDate,
            EventPurpose = r.EventPurpose,
            Notes = r.Notes,
            Status = StatusLabel(r.Status),
            LocationName = r.Location?.Name,
            CreatedAt = r.CreatedAt,
        };

    private static string? PrimaryImageUrl(ICollection<EventHallImage> images) =>
        images
            .OrderBy(i => i.SortOrder)
            .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
            .FirstOrDefault();

    private static string StatusLabel(EventHallRequestStatus status) =>
        status switch
        {
            EventHallRequestStatus.Approved => "Approved",
            EventHallRequestStatus.Rejected => "Rejected",
            EventHallRequestStatus.Cancelled => "Cancelled",
            _ => "Pending",
        };

    private static bool TryParseStatus(string? status, out EventHallRequestStatus parsed)
    {
        parsed = EventHallRequestStatus.Pending;
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        return TryParseStatusRequired(status, out parsed);
    }

    private static bool TryParseStatusRequired(string status, out EventHallRequestStatus parsed)
    {
        parsed = EventHallRequestStatus.Pending;
        if (string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            parsed = EventHallRequestStatus.Approved;
            return true;
        }

        if (string.Equals(status, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            parsed = EventHallRequestStatus.Rejected;
            return true;
        }

        if (string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "canceled", StringComparison.OrdinalIgnoreCase))
        {
            parsed = EventHallRequestStatus.Cancelled;
            return true;
        }

        if (string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase))
        {
            parsed = EventHallRequestStatus.Pending;
            return true;
        }

        return false;
    }

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

    private static bool ValidateRentalPrice(decimal price) => price >= 0 && price <= 99_999_999;

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
