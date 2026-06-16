using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/storefront-about")]
[Authorize(Roles = "Business")]
public sealed class BusinessStorefrontAboutController : ControllerBase
{
    private const int MaxUploadBytes = 8 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    private readonly IStorefrontAboutImageService _aboutImages;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BusinessStorefrontAboutController> _logger;

    public BusinessStorefrontAboutController(
        IStorefrontAboutImageService aboutImages,
        IWebHostEnvironment env,
        ILogger<BusinessStorefrontAboutController> logger)
    {
        _aboutImages = aboutImages;
        _env = env;
        _logger = logger;
    }

    [HttpGet("image")]
    [ProducesResponseType(typeof(ApiResult<StorefrontAboutImageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<StorefrontAboutImageDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var image = await _aboutImages.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<StorefrontAboutImageDto>.Ok(image is null ? null : MapAbsoluteUrl(image)));
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ApiResult<StorefrontAboutImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<StorefrontAboutImageDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<StorefrontAboutImageDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (file is null || file.Length <= 0)
        {
            return BadRequest(ApiResult<StorefrontAboutImageDto>.Fail("Validation", "No file uploaded."));
        }

        if (file.Length > MaxUploadBytes)
        {
            return BadRequest(ApiResult<StorefrontAboutImageDto>.Fail("Validation", "Image must be 8 MB or smaller."));
        }

        if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(ApiResult<StorefrontAboutImageDto>.Fail("Validation", "Only JPEG or PNG images are allowed."));
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = $"uploads/{businessId.Value:N}/about";
        var physicalDir = Path.Combine(webRoot, "uploads", businessId.Value.ToString("N"), "about");
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
            _logger.LogWarning(ex, "Failed saving about image for business {BusinessId}", businessId);
            return BadRequest(ApiResult<StorefrontAboutImageDto>.Fail("UploadFailed", "Could not save image."));
        }

        var dto = await _aboutImages
            .UpsertAsync(businessId.Value, relativePath, file.FileName, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return BadRequest(ApiResult<StorefrontAboutImageDto>.Fail("UploadFailed", "Could not register about image."));
        }

        return Ok(ApiResult<StorefrontAboutImageDto>.Ok(MapAbsoluteUrl(dto)));
    }

    [HttpDelete("image")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var deleted = await _aboutImages.DeleteAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (!deleted)
        {
            return NotFound(ApiResult.Fail("NotFound", "About image not found."));
        }

        return Ok(ApiResult.Ok("About image deleted."));
    }

    private StorefrontAboutImageDto MapAbsoluteUrl(StorefrontAboutImageDto image) =>
        new()
        {
            Id = image.Id,
            Url = ToAbsoluteUrl(image.Url) ?? image.Url,
            OriginalFileName = image.OriginalFileName,
        };

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
