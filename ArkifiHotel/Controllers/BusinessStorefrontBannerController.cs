using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/storefront-banner")]
[Authorize(Roles = "Business")]
public sealed class BusinessStorefrontBannerController : ControllerBase
{
    private const int MaxUploadBytes = 8 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    private readonly IStorefrontBannerImageService _bannerImages;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BusinessStorefrontBannerController> _logger;

    public BusinessStorefrontBannerController(
        IStorefrontBannerImageService bannerImages,
        IWebHostEnvironment env,
        ILogger<BusinessStorefrontBannerController> logger)
    {
        _bannerImages = bannerImages;
        _env = env;
        _logger = logger;
    }

    [HttpGet("images")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid? locationId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var images = await _bannerImages.GetAsync(businessId.Value, locationId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Ok(MapAbsoluteUrls(images)));
    }

    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes * IStorefrontBannerImageService.MaxImages)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImages(
        [FromForm] List<IFormFile> files,
        [FromForm] Guid locationId,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        if (locationId == Guid.Empty)
        {
            return BadRequest(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail("Validation", "Select a branch for these banner photos."));
        }

        if (files is null || files.Count == 0)
        {
            return BadRequest(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail("Validation", "No files uploaded."));
        }

        var existingCount = await _bannerImages.CountAsync(businessId.Value, locationId, cancellationToken).ConfigureAwait(false);
        if (existingCount >= IStorefrontBannerImageService.MaxImages)
        {
            return BadRequest(
                ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail(
                    "Validation",
                    $"You can upload up to {IStorefrontBannerImageService.MaxImages} banner images."));
        }

        if (existingCount + files.Count > IStorefrontBannerImageService.MaxImages)
        {
            return BadRequest(
                ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail(
                    "Validation",
                    $"Only {IStorefrontBannerImageService.MaxImages - existingCount} more banner image(s) can be added."));
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = $"uploads/{businessId.Value:N}/banner";
        var physicalDir = Path.Combine(webRoot, "uploads", businessId.Value.ToString("N"), "banner");
        Directory.CreateDirectory(physicalDir);

        foreach (var file in files)
        {
            if (file.Length <= 0 || file.Length > MaxUploadBytes)
            {
                continue;
            }

            if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
            {
                return BadRequest(
                    ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail(
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
                _logger.LogWarning(ex, "Failed saving banner image for business {BusinessId}", businessId);
                return BadRequest(
                    ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail("UploadFailed", "Could not save one or more images."));
            }

            var dto = await _bannerImages
                .AddImageAsync(businessId.Value, locationId, relativePath, file.FileName, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
            {
                return BadRequest(
                    ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Fail("UploadFailed", "Could not register banner image."));
            }
        }

        var all = await _bannerImages.GetAsync(businessId.Value, locationId, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<StorefrontBannerImageDto>>.Ok(MapAbsoluteUrls(all)));
    }

    [HttpDelete("images/{imageId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(Guid imageId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var deleted = await _bannerImages.DeleteAsync(businessId.Value, imageId, cancellationToken).ConfigureAwait(false);
        if (!deleted)
        {
            return NotFound(ApiResult.Fail("NotFound", "Banner image not found."));
        }

        return Ok(ApiResult.Ok("Banner image deleted."));
    }

    private IReadOnlyList<StorefrontBannerImageDto> MapAbsoluteUrls(IReadOnlyList<StorefrontBannerImageDto> images) =>
        images
            .Select(i => new StorefrontBannerImageDto
            {
                Id = i.Id,
                Url = ToAbsoluteUrl(i.Url) ?? i.Url,
                OriginalFileName = i.OriginalFileName,
                SortOrder = i.SortOrder,
                LocationId = i.LocationId,
                LocationName = i.LocationName,
            })
            .ToList();

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
