using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessRoomService : IBusinessRoomService
{
    private readonly AdminDbContext _db;
    private readonly IWebHostEnvironment _env;

    public BusinessRoomService(AdminDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IReadOnlyList<BusinessRoomSummaryDto>> ListAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var rooms = await _db.Rooms
            .AsNoTracking()
            .Include(r => r.Images)
            .Include(r => r.RoomAmenities)
            .Where(r => r.BusinessRegistrationId == businessId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rooms
            .Select(r => new BusinessRoomSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                MaxOccupancy = r.MaxOccupancy,
                BasePricePerNight = r.BasePricePerNight,
                PrimaryImageUrl = r.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal))
                    .FirstOrDefault(),
                AmenityCount = r.RoomAmenities.Count,
            })
            .ToList();
    }

    public async Task<BusinessRoomDetailDto?> GetAsync(
        Guid businessId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var room = await _db.Rooms
            .AsNoTracking()
            .Include(r => r.Images)
            .Include(r => r.RoomAmenities)
            .ThenInclude(ra => ra.Amenity)
            .FirstOrDefaultAsync(r => r.Id == roomId && r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return room is null ? null : MapDetail(room);
    }

    public async Task<BusinessRoomDetailDto?> CreateAsync(
        Guid businessId,
        CreateBusinessRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRoomCore(request.Name, request.Description, request.MaxOccupancy, request.BasePricePerNight);
        if (validation is not null)
        {
            return null;
        }

        if (!await BusinessExistsAsync(businessId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var amenityIds = request.AmenityIds ?? Array.Empty<Guid>();
        if (!await AmenitiesAllowedForBusinessAsync(businessId, amenityIds, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var room = new Room
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            Name = request.Name!.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            MaxOccupancy = request.MaxOccupancy,
            BasePricePerNight = request.BasePricePerNight,
            CreatedAt = now,
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await ReplaceRoomAmenitiesAsync(room.Id, amenityIds, cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, room.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<BusinessRoomDetailDto?> UpdateAsync(
        Guid businessId,
        Guid roomId,
        UpdateBusinessRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateRoomCore(request.Name, request.Description, request.MaxOccupancy, request.BasePricePerNight);
        if (validation is not null)
        {
            return null;
        }

        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (room is null)
        {
            return null;
        }

        var amenityIds = request.AmenityIds ?? Array.Empty<Guid>();
        if (!await AmenitiesAllowedForBusinessAsync(businessId, amenityIds, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        room.Name = request.Name!.Trim();
        room.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        room.MaxOccupancy = request.MaxOccupancy;
        room.BasePricePerNight = request.BasePricePerNight;
        room.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await ReplaceRoomAmenitiesAsync(room.Id, amenityIds, cancellationToken).ConfigureAwait(false);

        return await GetAsync(businessId, room.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(Guid businessId, Guid roomId, CancellationToken cancellationToken = default)
    {
        var room = await _db.Rooms
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == roomId && r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (room is null)
        {
            return false;
        }

        foreach (var img in room.Images)
        {
            TryDeletePhysicalFile(img.RelativePath);
        }

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<RoomImageDto?> AddImageAsync(
        Guid businessId,
        Guid roomId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRelativePath(relativePathUnderWwwroot);
        if (normalized is null)
        {
            return null;
        }

        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (room is null)
        {
            return null;
        }

        var maxOrder = await _db.RoomImages
            .Where(i => i.RoomId == roomId)
            .Select(i => (int?)i.SortOrder)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? -1;

        var entity = new RoomImage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            RelativePath = normalized,
            OriginalFileName = SanitizeOriginalFileName(originalFileName),
            SortOrder = maxOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.RoomImages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapImage(entity);
    }

    public async Task<bool> DeleteImageAsync(
        Guid businessId,
        Guid roomId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var image = await _db.RoomImages
            .Include(i => i.Room)
            .FirstOrDefaultAsync(
                i => i.Id == imageId && i.RoomId == roomId && i.Room.BusinessRegistrationId == businessId,
                cancellationToken)
            .ConfigureAwait(false);

        if (image is null)
        {
            return false;
        }

        TryDeletePhysicalFile(image.RelativePath);
        _db.RoomImages.Remove(image);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<bool> BusinessExistsAsync(Guid businessId, CancellationToken cancellationToken) =>
        await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

    private async Task<bool> AmenitiesAllowedForBusinessAsync(
        Guid businessId,
        IReadOnlyList<Guid> amenityIds,
        CancellationToken cancellationToken)
    {
        if (amenityIds.Count == 0)
        {
            return true;
        }

        var distinct = amenityIds.Distinct().ToList();
        var allowed = await _db.Amenities
            .AsNoTracking()
            .Where(a => distinct.Contains(a.Id) && (a.BusinessRegistrationId == null || a.BusinessRegistrationId == businessId))
            .Select(a => a.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return allowed.Count == distinct.Count;
    }

    private async Task ReplaceRoomAmenitiesAsync(
        Guid roomId,
        IReadOnlyList<Guid> amenityIds,
        CancellationToken cancellationToken)
    {
        var existing = await _db.RoomAmenities.Where(ra => ra.RoomId == roomId).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (existing.Count > 0)
        {
            _db.RoomAmenities.RemoveRange(existing);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var aid in amenityIds.Distinct())
        {
            _db.RoomAmenities.Add(
                new RoomAmenity
                {
                    RoomId = roomId,
                    AmenityId = aid,
                });
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string? ValidateRoomCore(string? name, string? description, int maxOccupancy, decimal basePrice)
    {
        var n = name?.Trim() ?? string.Empty;
        if (n.Length < 2 || n.Length > 200)
        {
            return "bad";
        }

        if (!string.IsNullOrEmpty(description) && description.Trim().Length > 4000)
        {
            return "bad";
        }

        if (maxOccupancy < 1 || maxOccupancy > 50)
        {
            return "bad";
        }

        if (basePrice < 0 || basePrice > 1_000_000)
        {
            return "bad";
        }

        return null;
    }

    private static BusinessRoomDetailDto MapDetail(Room r)
    {
        var images = r.Images
            .OrderBy(i => i.SortOrder)
            .Select(MapImage)
            .ToList();

        var amenities = r.RoomAmenities
            .Select(ra => MapAmenity(ra.Amenity))
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Name)
            .ToList();

        return new BusinessRoomDetailDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            MaxOccupancy = r.MaxOccupancy,
            BasePricePerNight = r.BasePricePerNight,
            Images = images,
            Amenities = amenities,
        };
    }

    private static RoomImageDto MapImage(RoomImage i) =>
        new()
        {
            Id = i.Id,
            Url = "/" + i.RelativePath.Replace("\\", "/", StringComparison.Ordinal),
            OriginalFileName = i.OriginalFileName,
            SortOrder = i.SortOrder,
        };

    private static AmenityDto MapAmenity(Amenity a) =>
        new()
        {
            Id = a.Id,
            Name = a.Name,
            Category = a.Category,
            IsCustom = a.BusinessRegistrationId.HasValue,
        };

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
            // ignore IO errors; DB row still removed elsewhere
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
