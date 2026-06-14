using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/rooms")]
[Authorize(Roles = "Business")]
public sealed class BusinessRoomsController : ControllerBase
{
    private const long MaxUploadBytes = 8 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    private readonly IBusinessRoomService _rooms;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BusinessRoomsController> _logger;

    public BusinessRoomsController(
        IBusinessRoomService rooms,
        IWebHostEnvironment env,
        ILogger<BusinessRoomsController> logger)
    {
        _rooms = rooms;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<BusinessRoomSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<BusinessRoomSummaryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _rooms.ListAsync(businessId.Value, includeArchived, cancellationToken).ConfigureAwait(false);
        var mapped = list
            .Select(r => new BusinessRoomSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                MaxOccupancy = r.MaxOccupancy,
                BasePricePerNight = r.BasePricePerNight,
                Quantity = r.Quantity,
                LocationId = r.LocationId,
                LocationName = r.LocationName,
                PrimaryImageUrl = string.IsNullOrWhiteSpace(r.PrimaryImageUrl) ? null : ToAbsoluteUrl(r.PrimaryImageUrl),
                AmenityCount = r.AmenityCount,
                IsArchived = r.IsArchived,
            })
            .ToList();
        return Ok(ApiResult<IReadOnlyList<BusinessRoomSummaryDto>>.Ok(mapped));
    }

    [HttpGet("{roomId:guid}")]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid roomId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessRoomDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var room = await _rooms.GetAsync(businessId.Value, roomId, cancellationToken).ConfigureAwait(false);
        if (room is null)
        {
            return NotFound(ApiResult<BusinessRoomDetailDto>.Fail("NotFound", "Room not found."));
        }

        return Ok(ApiResult<BusinessRoomDetailDto>.Ok(MapRoom(room)));
    }

    [HttpPost("{roomId:guid}/archive")]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid roomId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessRoomDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _rooms.SetArchivedAsync(businessId.Value, roomId, true, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult<BusinessRoomDetailDto>.Fail("NotFound", "Room not found."));
        }

        var room = await _rooms.GetAsync(businessId.Value, roomId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<BusinessRoomDetailDto>.Ok(MapRoom(room!)));
    }

    [HttpPost("{roomId:guid}/restore")]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(Guid roomId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessRoomDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _rooms.SetArchivedAsync(businessId.Value, roomId, false, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult<BusinessRoomDetailDto>.Fail("NotFound", "Room not found."));
        }

        var room = await _rooms.GetAsync(businessId.Value, roomId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<BusinessRoomDetailDto>.Ok(MapRoom(room!)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBusinessRoomRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessRoomDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var room = await _rooms.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (room is null)
        {
            return BadRequest(
                ApiResult<BusinessRoomDetailDto>.Fail(
                    "Validation",
                    "Could not create room. Check fields, select a location, and amenity selections."));
        }

        return Created($"/api/business/rooms/{room.Id}", ApiResult<BusinessRoomDetailDto>.Ok(MapRoom(room)));
    }

    [HttpPut("{roomId:guid}")]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BusinessRoomDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid roomId,
        [FromBody] UpdateBusinessRoomRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessRoomDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var room = await _rooms.UpdateAsync(businessId.Value, roomId, request, cancellationToken).ConfigureAwait(false);
        if (room is null)
        {
            var exists = await _rooms.GetAsync(businessId.Value, roomId, cancellationToken).ConfigureAwait(false);
            if (exists is null)
            {
                return NotFound(ApiResult<BusinessRoomDetailDto>.Fail("NotFound", "Room not found."));
            }

            return BadRequest(
                ApiResult<BusinessRoomDetailDto>.Fail(
                    "Validation",
                    "Could not update room. Check fields, select a location, and amenity selections."));
        }

        return Ok(ApiResult<BusinessRoomDetailDto>.Ok(MapRoom(room)));
    }

    [HttpDelete("{roomId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid roomId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _rooms.DeleteAsync(businessId.Value, roomId, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult.Fail("NotFound", "Room not found."));
        }

        return Ok(ApiResult.Ok("Room deleted."));
    }

    [HttpPost("{roomId:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RoomImageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RoomImageDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImages(
        Guid roomId,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<RoomImageDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        if (files is null || files.Count == 0)
        {
            return BadRequest(ApiResult<IReadOnlyList<RoomImageDto>>.Fail("Validation", "No files uploaded."));
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = $"uploads/{businessId.Value:N}/{roomId:N}";
        var physicalDir = Path.Combine(webRoot, "uploads", businessId.Value.ToString("N"), roomId.ToString("N"));
        Directory.CreateDirectory(physicalDir);

        var saved = new List<RoomImageDto>();
        foreach (var file in files)
        {
            if (file.Length <= 0 || file.Length > MaxUploadBytes)
            {
                continue;
            }

            if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
            {
                return BadRequest(
                    ApiResult<IReadOnlyList<RoomImageDto>>.Fail(
                        "Validation",
                        "Only JPEG or PNG images are allowed."));
            }

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || ext.Length > 10)
            {
                ext = file.ContentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    _ => ".bin",
                };
            }

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var relativePath = $"{relativeFolder}/{fileName}";
            var physicalPath = Path.Combine(physicalDir, fileName);

            try
            {
                await using (var stream = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed saving room image for room {RoomId}", roomId);
                return BadRequest(ApiResult<IReadOnlyList<RoomImageDto>>.Fail("UploadFailed", "Could not save one or more images."));
            }

            var dto = await _rooms
                .AddImageAsync(businessId.Value, roomId, relativePath, file.FileName, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
            {
                TryDeleteFile(physicalPath);
                return BadRequest(ApiResult<IReadOnlyList<RoomImageDto>>.Fail("Validation", "Room not found or invalid upload path."));
            }

            saved.Add(dto);
        }

        if (saved.Count == 0)
        {
            return BadRequest(ApiResult<IReadOnlyList<RoomImageDto>>.Fail("Validation", "No valid image files were processed."));
        }

        var mapped = saved.Select(MapImage).ToList();
        return Ok(ApiResult<IReadOnlyList<RoomImageDto>>.Ok(mapped));
    }

    private string ToAbsoluteUrl(string urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath))
        {
            return urlOrPath;
        }

        if (urlOrPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || urlOrPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return urlOrPath;
        }

        var path = urlOrPath.StartsWith('/') ? urlOrPath : "/" + urlOrPath;
        return $"{Request.Scheme}://{Request.Host}{path}";
    }

    private BusinessRoomDetailDto MapRoom(BusinessRoomDetailDto room) =>
        new()
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            MaxOccupancy = room.MaxOccupancy,
            BasePricePerNight = room.BasePricePerNight,
            Quantity = room.Quantity,
            LocationId = room.LocationId,
            LocationName = room.LocationName,
            Images = room.Images.Select(MapImage).ToList(),
            Amenities = room.Amenities,
            IsArchived = room.IsArchived,
        };

    private RoomImageDto MapImage(RoomImageDto img) =>
        new()
        {
            Id = img.Id,
            Url = ToAbsoluteUrl(img.Url),
            OriginalFileName = img.OriginalFileName,
            SortOrder = img.SortOrder,
        };

    [HttpDelete("{roomId:guid}/images/{imageId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(Guid roomId, Guid imageId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _rooms.DeleteImageAsync(businessId.Value, roomId, imageId, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult.Fail("NotFound", "Image not found."));
        }

        return Ok(ApiResult.Ok("Image removed."));
    }

    private static void TryDeleteFile(string physicalPath)
    {
        try
        {
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }
        catch
        {
            // ignore
        }
    }
}
