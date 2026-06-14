using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/profile")]
[Authorize(Roles = "Business")]
public sealed class BusinessProfileController : ControllerBase
{
    private const long MaxUploadBytes = 8 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
    };

    private readonly IBusinessProfileService _profile;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BusinessProfileController> _logger;

    public BusinessProfileController(
        IBusinessProfileService profile,
        IWebHostEnvironment env,
        ILogger<BusinessProfileController> logger)
    {
        _profile = profile;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessProfileDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _profile.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<BusinessProfileDto>.Fail("NotFound", "Business not found."));
        }

        return Ok(ApiResult<BusinessProfileDto>.Ok(MapProfile(dto)));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromBody] UpdateBusinessProfileRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessProfileDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, error, message) = await _profile.UpdateAsync(businessId.Value, request, cancellationToken)
            .ConfigureAwait(false);

        return error switch
        {
            BusinessProfileUpdateError.NotFound => NotFound(ApiResult<BusinessProfileDto>.Fail("NotFound", message ?? "Business not found.")),
            BusinessProfileUpdateError.Validation => BadRequest(ApiResult<BusinessProfileDto>.Fail("Validation", message ?? "Invalid profile data.")),
            BusinessProfileUpdateError.DuplicateSlug => Conflict(ApiResult<BusinessProfileDto>.Fail("DuplicateSlug", message ?? "This slug is already taken.")),
            _ => Ok(ApiResult<BusinessProfileDto>.Ok(MapProfile(data!))),
        };
    }

    [HttpPost("logo")]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessProfileDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (file is null || file.Length <= 0)
        {
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("Validation", "No logo file uploaded."));
        }

        if (file.Length > MaxUploadBytes)
        {
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("Validation", "Logo must be 8MB or smaller."));
        }

        if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("Validation", "Only JPEG or PNG images are allowed."));
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

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var relativeFolder = $"uploads/{businessId.Value:N}/logo";
        var physicalDir = Path.Combine(webRoot, "uploads", businessId.Value.ToString("N"), "logo");
        Directory.CreateDirectory(physicalDir);

        var existing = await _profile.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (existing?.LogoUrl is not null)
        {
            TryDeleteStoredFile(webRoot, existing.LogoUrl);
        }

        var fileName = $"logo{ext}";
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
            _logger.LogWarning(ex, "Failed saving business logo for {BusinessId}", businessId);
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("UploadFailed", "Could not save logo."));
        }

        var (data, error, message) = await _profile
            .UpdateLogoPathAsync(businessId.Value, relativePath, cancellationToken)
            .ConfigureAwait(false);

        if (error != BusinessProfileUpdateError.None || data is null)
        {
            TryDeleteFile(physicalPath);
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("UploadFailed", message ?? "Could not update profile."));
        }

        return Ok(ApiResult<BusinessProfileDto>.Ok(MapProfile(data)));
    }

    [HttpDelete("logo")]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveLogo(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessProfileDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var existing = await _profile.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (existing?.LogoUrl is not null)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            TryDeleteStoredFile(webRoot, existing.LogoUrl);
        }

        var (data, error, message) = await _profile
            .UpdateLogoPathAsync(businessId.Value, relativeLogoPath: null, cancellationToken)
            .ConfigureAwait(false);

        if (error != BusinessProfileUpdateError.None || data is null)
        {
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("Validation", message ?? "Could not remove logo."));
        }

        return Ok(ApiResult<BusinessProfileDto>.Ok(MapProfile(data)));
    }

    [HttpGet("check-slug")]
    [ProducesResponseType(typeof(ApiResult<SlugAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSlug([FromQuery] string slug, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<SlugAvailabilityDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var result = await _profile
            .CheckSlugAvailabilityAsync(slug, businessId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(ApiResult<SlugAvailabilityDto>.Ok(result));
    }

    private BusinessProfileDto MapProfile(BusinessProfileDto dto) =>
        new()
        {
            Id = dto.Id,
            BusinessName = dto.BusinessName,
            Slug = dto.Slug,
            LogoUrl = ToAbsoluteUrl(dto.LogoUrl),
            ContactEmail = dto.ContactEmail,
            PhoneNumber = dto.PhoneNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsEmailVerified = dto.IsEmailVerified,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
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

    private static void TryDeleteStoredFile(string webRoot, string relativeOrAbsolutePath)
    {
        var path = relativeOrAbsolutePath;
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(path);
            path = uri.AbsolutePath.TrimStart('/');
        }

        path = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        TryDeleteFile(Path.Combine(webRoot, path));
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
            // Best effort cleanup.
        }
    }
}
