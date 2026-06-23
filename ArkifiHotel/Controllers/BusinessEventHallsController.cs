using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/event-halls")]
[Authorize(Roles = "Business")]
public sealed class BusinessEventHallsController : ControllerBase
{
    private const long MaxUploadBytes = 8 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    private readonly IBusinessEventHallService _eventHalls;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BusinessEventHallsController> _logger;

    public BusinessEventHallsController(
        IBusinessEventHallService eventHalls,
        IWebHostEnvironment env,
        ILogger<BusinessEventHallsController> logger)
    {
        _eventHalls = eventHalls;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<EventHallSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(
                ApiResult<IReadOnlyList<EventHallSummaryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _eventHalls.ListAsync(businessId.Value, includeArchived, cancellationToken).ConfigureAwait(false);
        var mapped = list
            .Select(h => new EventHallSummaryDto
            {
                Id = h.Id,
                Name = h.Name,
                RentalPrice = h.RentalPrice,
                MaxCapacity = h.MaxCapacity,
                PrimaryImageUrl = ToAbsoluteUrl(h.PrimaryImageUrl),
                ImageCount = h.ImageCount,
                LocationId = h.LocationId,
                LocationName = h.LocationName,
                IsArchived = h.IsArchived,
            })
            .ToList();

        return Ok(ApiResult<IReadOnlyList<EventHallSummaryDto>>.Ok(mapped));
    }

    [HttpGet("{eventHallId:guid}")]
    [ProducesResponseType(typeof(ApiResult<EventHallDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<EventHallDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid eventHallId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<EventHallDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _eventHalls.GetAsync(businessId.Value, eventHallId, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<EventHallDetailDto>.Fail("NotFound", "Event hall not found."));
        }

        return Ok(ApiResult<EventHallDetailDto>.Ok(MapDetail(dto)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<EventHallDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<EventHallDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventHallRequest request, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<EventHallDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _eventHalls.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return BadRequest(
                ApiResult<EventHallDetailDto>.Fail(
                    "Validation",
                    "Could not create event hall. Check name, rental price, and branch."));
        }

        return Created($"/api/business/event-halls/{dto.Id}", ApiResult<EventHallDetailDto>.Ok(MapDetail(dto)));
    }

    [HttpPut("{eventHallId:guid}")]
    [ProducesResponseType(typeof(ApiResult<EventHallDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<EventHallDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid eventHallId,
        [FromBody] UpdateEventHallRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<EventHallDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _eventHalls.UpdateAsync(businessId.Value, eventHallId, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            var exists = await _eventHalls.GetAsync(businessId.Value, eventHallId, cancellationToken).ConfigureAwait(false);
            return exists is null
                ? NotFound(ApiResult<EventHallDetailDto>.Fail("NotFound", "Event hall not found."))
                : BadRequest(ApiResult<EventHallDetailDto>.Fail("Validation", "Could not update event hall."));
        }

        return Ok(ApiResult<EventHallDetailDto>.Ok(MapDetail(dto)));
    }

    [HttpPost("{eventHallId:guid}/archive")]
    public async Task<IActionResult> Archive(Guid eventHallId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _eventHalls.ArchiveAsync(businessId.Value, eventHallId, cancellationToken).ConfigureAwait(false);
        return ok
            ? Ok(ApiResult.Ok("Event hall archived."))
            : NotFound(ApiResult.Fail("NotFound", "Event hall not found."));
    }

    [HttpPost("{eventHallId:guid}/restore")]
    public async Task<IActionResult> Restore(Guid eventHallId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _eventHalls.RestoreAsync(businessId.Value, eventHallId, cancellationToken).ConfigureAwait(false);
        return ok
            ? Ok(ApiResult.Ok("Event hall restored."))
            : NotFound(ApiResult.Fail("NotFound", "Event hall not found."));
    }

    [HttpDelete("{eventHallId:guid}")]
    public async Task<IActionResult> Delete(Guid eventHallId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _eventHalls.DeleteAsync(businessId.Value, eventHallId, cancellationToken).ConfigureAwait(false);
        return ok
            ? Ok(ApiResult.Ok("Event hall deleted."))
            : NotFound(ApiResult.Fail("NotFound", "Event hall not found."));
    }

    [HttpPost("{eventHallId:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ApiResult<EventHallDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadImage(
        Guid eventHallId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<EventHallDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResult<EventHallDetailDto>.Fail("Validation", "No file uploaded."));
        }

        if (!TryValidateImage(file, out var error))
        {
            return BadRequest(ApiResult<EventHallDetailDto>.Fail("Validation", error));
        }

        var relativePath = await SaveUploadAsync(businessId.Value, eventHallId, file, cancellationToken).ConfigureAwait(false);
        if (relativePath is null)
        {
            return BadRequest(ApiResult<EventHallDetailDto>.Fail("UploadFailed", "Could not save image."));
        }

        var dto = await _eventHalls
            .AddImageAsync(businessId.Value, eventHallId, relativePath, file.FileName, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return NotFound(ApiResult<EventHallDetailDto>.Fail("NotFound", "Event hall not found."));
        }

        return Ok(ApiResult<EventHallDetailDto>.Ok(MapDetail(dto)));
    }

    [HttpDelete("{eventHallId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(
        Guid eventHallId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _eventHalls
            .RemoveImageAsync(businessId.Value, eventHallId, imageId, cancellationToken)
            .ConfigureAwait(false);

        return ok
            ? Ok(ApiResult.Ok("Image removed."))
            : NotFound(ApiResult.Fail("NotFound", "Image not found."));
    }

    [HttpGet("requests")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<EventHallRequestListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRequests([FromQuery] string? status, CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(
                ApiResult<IReadOnlyList<EventHallRequestListItemDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _eventHalls.ListRequestsAsync(businessId.Value, status, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<EventHallRequestListItemDto>>.Ok(list));
    }

    [HttpGet("requests/{requestId:guid}")]
    [ProducesResponseType(typeof(ApiResult<EventHallRequestDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<EventHallRequestDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<EventHallRequestDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _eventHalls.GetRequestAsync(businessId.Value, requestId, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<EventHallRequestDetailDto>.Fail("NotFound", "Request not found."));
        }

        return Ok(ApiResult<EventHallRequestDetailDto>.Ok(dto));
    }

    [HttpPut("requests/{requestId:guid}/status")]
    [ProducesResponseType(typeof(ApiResult<EventHallRequestDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<EventHallRequestDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRequestStatus(
        Guid requestId,
        [FromBody] UpdateEventHallRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<EventHallRequestDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _eventHalls
            .UpdateRequestStatusAsync(businessId.Value, requestId, request, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            var exists = await _eventHalls.GetRequestAsync(businessId.Value, requestId, cancellationToken).ConfigureAwait(false);
            return exists is null
                ? NotFound(ApiResult<EventHallRequestDetailDto>.Fail("NotFound", "Request not found."))
                : BadRequest(ApiResult<EventHallRequestDetailDto>.Fail("Validation", "Invalid status."));
        }

        return Ok(ApiResult<EventHallRequestDetailDto>.Ok(dto));
    }

    private EventHallDetailDto MapDetail(EventHallDetailDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            RentalPrice = dto.RentalPrice,
            MaxCapacity = dto.MaxCapacity,
            LocationId = dto.LocationId,
            LocationName = dto.LocationName,
            IsArchived = dto.IsArchived,
            Images = dto.Images
                .Select(i => new EventHallImageDto
                {
                    Id = i.Id,
                    Url = ToAbsoluteUrl(i.Url) ?? i.Url,
                    OriginalFileName = i.OriginalFileName,
                    SortOrder = i.SortOrder,
                })
                .ToList(),
        };

    private bool TryValidateImage(IFormFile file, out string error)
    {
        error = string.Empty;
        if (file.Length > MaxUploadBytes)
        {
            error = "Image must be 8 MB or smaller.";
            return false;
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            error = "Only JPEG or PNG images are allowed.";
            return false;
        }

        return true;
    }

    private async Task<string?> SaveUploadAsync(
        Guid businessId,
        Guid eventHallId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = Path.Combine("uploads", businessId.ToString("N"), "event-halls", eventHallId.ToString("N"))
            .Replace('\\', '/');
        var physicalDir = Path.Combine(webRoot, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(physicalDir);

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
        var physicalPath = Path.Combine(physicalDir, fileName);

        try
        {
            await using var stream = System.IO.File.Create(physicalPath);
            await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            return $"{relativeFolder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed saving event hall image for business {BusinessId}", businessId);
            return null;
        }
    }

    private string? ToAbsoluteUrl(string? urlOrPath)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath))
        {
            return null;
        }

        if (urlOrPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || urlOrPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return urlOrPath;
        }

        var path = urlOrPath.StartsWith('/') ? urlOrPath : "/" + urlOrPath;
        return $"{Request.Scheme}://{Request.Host}{path}";
    }
}
