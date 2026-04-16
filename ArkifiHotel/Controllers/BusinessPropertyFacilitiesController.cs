using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/facilities")]
[Authorize(Roles = "Business")]
public sealed class BusinessPropertyFacilitiesController : ControllerBase
{
    private const long MaxUploadBytes = 8 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
    };

    private readonly IBusinessPropertyFacilityService _facilities;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BusinessPropertyFacilitiesController> _logger;

    public BusinessPropertyFacilitiesController(
        IBusinessPropertyFacilityService facilities,
        IWebHostEnvironment env,
        ILogger<BusinessPropertyFacilitiesController> logger)
    {
        _facilities = facilities;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<PropertyFacilitySummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(
                ApiResult<IReadOnlyList<PropertyFacilitySummaryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _facilities.ListAsync(businessId.Value, includeArchived, cancellationToken).ConfigureAwait(false);
        var mapped = list
            .Select(f => new PropertyFacilitySummaryDto
            {
                Id = f.Id,
                Name = f.Name,
                PrimaryImageUrl = string.IsNullOrWhiteSpace(f.PrimaryImageUrl) ? null : ToAbsoluteUrl(f.PrimaryImageUrl),
                ImageCount = f.ImageCount,
                IsArchived = f.IsArchived,
            })
            .ToList();
        return Ok(ApiResult<IReadOnlyList<PropertyFacilitySummaryDto>>.Ok(mapped));
    }

    [HttpGet("{facilityId:guid}")]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid facilityId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PropertyFacilityDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _facilities.GetAsync(businessId.Value, facilityId, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<PropertyFacilityDetailDto>.Fail("NotFound", "Facility not found."));
        }

        return Ok(ApiResult<PropertyFacilityDetailDto>.Ok(MapFacility(dto)));
    }

    [HttpPost("{facilityId:guid}/archive")]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid facilityId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PropertyFacilityDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _facilities.SetArchivedAsync(businessId.Value, facilityId, true, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult<PropertyFacilityDetailDto>.Fail("NotFound", "Facility not found."));
        }

        var dto = await _facilities.GetAsync(businessId.Value, facilityId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<PropertyFacilityDetailDto>.Ok(MapFacility(dto!)));
    }

    [HttpPost("{facilityId:guid}/restore")]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(Guid facilityId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PropertyFacilityDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _facilities.SetArchivedAsync(businessId.Value, facilityId, false, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult<PropertyFacilityDetailDto>.Fail("NotFound", "Facility not found."));
        }

        var dto = await _facilities.GetAsync(businessId.Value, facilityId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<PropertyFacilityDetailDto>.Ok(MapFacility(dto!)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePropertyFacilityRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PropertyFacilityDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _facilities.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return BadRequest(
                ApiResult<PropertyFacilityDetailDto>.Fail(
                    "Validation",
                    "Could not create facility. Check the name (2–200 characters) and description length."));
        }

        return Created($"/api/business/facilities/{dto.Id}", ApiResult<PropertyFacilityDetailDto>.Ok(MapFacility(dto)));
    }

    [HttpPut("{facilityId:guid}")]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<PropertyFacilityDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid facilityId,
        [FromBody] UpdatePropertyFacilityRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PropertyFacilityDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _facilities.UpdateAsync(businessId.Value, facilityId, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            var exists = await _facilities.GetAsync(businessId.Value, facilityId, cancellationToken).ConfigureAwait(false);
            if (exists is null)
            {
                return NotFound(ApiResult<PropertyFacilityDetailDto>.Fail("NotFound", "Facility not found."));
            }

            return BadRequest(
                ApiResult<PropertyFacilityDetailDto>.Fail(
                    "Validation",
                    "Could not update facility. Check the name and description."));
        }

        return Ok(ApiResult<PropertyFacilityDetailDto>.Ok(MapFacility(dto)));
    }

    [HttpDelete("{facilityId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid facilityId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _facilities.DeleteAsync(businessId.Value, facilityId, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult.Fail("NotFound", "Facility not found."));
        }

        return Ok(ApiResult.Ok("Facility deleted."));
    }

    [HttpPost("{facilityId:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<FacilityImageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<FacilityImageDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImages(
        Guid facilityId,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(
                ApiResult<IReadOnlyList<FacilityImageDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        if (files is null || files.Count == 0)
        {
            return BadRequest(ApiResult<IReadOnlyList<FacilityImageDto>>.Fail("Validation", "No files uploaded."));
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = $"uploads/{businessId.Value:N}/facilities/{facilityId:N}";
        var physicalDir = Path.Combine(
            webRoot,
            "uploads",
            businessId.Value.ToString("N"),
            "facilities",
            facilityId.ToString("N"));
        Directory.CreateDirectory(physicalDir);

        var saved = new List<FacilityImageDto>();
        foreach (var file in files)
        {
            if (file.Length <= 0 || file.Length > MaxUploadBytes)
            {
                continue;
            }

            if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
            {
                return BadRequest(
                    ApiResult<IReadOnlyList<FacilityImageDto>>.Fail(
                        "Validation",
                        "Only JPEG, PNG, WebP, or GIF images are allowed."));
            }

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || ext.Length > 10)
            {
                ext = file.ContentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
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
                _logger.LogWarning(ex, "Failed saving facility image for {FacilityId}", facilityId);
                return BadRequest(
                    ApiResult<IReadOnlyList<FacilityImageDto>>.Fail("UploadFailed", "Could not save one or more images."));
            }

            var dto = await _facilities
                .AddImageAsync(businessId.Value, facilityId, relativePath, file.FileName, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
            {
                TryDeleteFile(physicalPath);
                return BadRequest(
                    ApiResult<IReadOnlyList<FacilityImageDto>>.Fail(
                        "Validation",
                        "Facility not found or invalid upload path."));
            }

            saved.Add(dto);
        }

        if (saved.Count == 0)
        {
            return BadRequest(
                ApiResult<IReadOnlyList<FacilityImageDto>>.Fail("Validation", "No valid image files were processed."));
        }

        var mapped = saved.Select(MapImage).ToList();
        return Ok(ApiResult<IReadOnlyList<FacilityImageDto>>.Ok(mapped));
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

    private PropertyFacilityDetailDto MapFacility(PropertyFacilityDetailDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Images = dto.Images.Select(MapImage).ToList(),
            IsArchived = dto.IsArchived,
        };

    private FacilityImageDto MapImage(FacilityImageDto img) =>
        new()
        {
            Id = img.Id,
            Url = ToAbsoluteUrl(img.Url),
            OriginalFileName = img.OriginalFileName,
            SortOrder = img.SortOrder,
        };

    [HttpDelete("{facilityId:guid}/images/{imageId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(Guid facilityId, Guid imageId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _facilities.DeleteImageAsync(businessId.Value, facilityId, imageId, cancellationToken).ConfigureAwait(false);
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
