using Admin.Infrastructure.Helpers;
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
    private const long MaxUploadBytes = BusinessLogoFileHelper.MaxUploadBytes;

    private readonly IBusinessProfileService _profile;
    private readonly IWebHostEnvironment _env;

    public BusinessProfileController(
        IBusinessProfileService profile,
        IWebHostEnvironment env)
    {
        _profile = profile;
        _env = env;
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
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessProfileDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken cancellationToken)
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

        var validationError = BusinessLogoFileHelper.Validate(file);
        if (validationError is not null)
        {
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("Validation", validationError));
        }

        var existing = await _profile.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (existing?.LogoUrl is not null)
        {
            BusinessLogoFileHelper.TryDeleteStoredFile(_env, existing.LogoUrl);
        }

        var (relativePath, saveError) = await BusinessLogoFileHelper
            .SaveAsync(_env, businessId.Value, file, cancellationToken)
            .ConfigureAwait(false);

        if (saveError is not null || relativePath is null)
        {
            return BadRequest(ApiResult<BusinessProfileDto>.Fail("UploadFailed", saveError ?? "Could not save logo."));
        }

        var (data, error, message) = await _profile
            .UpdateLogoPathAsync(businessId.Value, relativePath, cancellationToken)
            .ConfigureAwait(false);

        if (error != BusinessProfileUpdateError.None || data is null)
        {
            BusinessLogoFileHelper.TryDeleteStoredFile(_env, relativePath);
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
            BusinessLogoFileHelper.TryDeleteStoredFile(_env, existing.LogoUrl);
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
            BusinessType = dto.BusinessType,
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
}
