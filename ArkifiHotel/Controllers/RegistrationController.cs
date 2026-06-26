using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IBusinessRegistrationService _registrationService;
    private readonly IBusinessProfileService _profileService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(
        IBusinessRegistrationService registrationService,
        IBusinessProfileService profileService,
        IWebHostEnvironment env,
        ILogger<RegistrationController> logger)
    {
        _registrationService = registrationService;
        _profileService = profileService;
        _env = env;
        _logger = logger;
    }

    /// <summary>Check whether a hotel slug is available for registration.</summary>
    [HttpGet("check-slug")]
    [ProducesResponseType(typeof(ApiResult<SlugAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSlug([FromQuery] string slug, CancellationToken cancellationToken)
    {
        var result = await _profileService.CheckSlugAvailabilityAsync(slug, excludeBusinessId: null, cancellationToken)
            .ConfigureAwait(false);
        return Ok(ApiResult<SlugAvailabilityDto>.Ok(result));
    }

    /// <summary>Register a new business (hotel / shortlet) account. Accepts multipart form with optional logo.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(BusinessLogoFileHelper.MaxUploadBytes + 64_000)]
    [ProducesResponseType(typeof(ApiResult<BusinessRegistrationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<BusinessRegistrationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BusinessRegistrationDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromForm] RegisterBusinessFormRequest form,
        IFormFile? logo,
        CancellationToken cancellationToken)
    {
        var result = await _registrationService.RegisterAsync(form.ToRegisterBusinessRequest(), cancellationToken)
            .ConfigureAwait(false);
        var api = result.ToApiResult();

        if (!api.Success || api.Data is null)
        {
            var code = result.ErrorCode ?? "Error";
            return code switch
            {
                "DuplicateEmail" => Conflict(api),
                "DuplicateSlug" => Conflict(api),
                _ => BadRequest(api),
            };
        }

        if (logo is { Length: > 0 })
        {
            var (relativePath, logoError) = await BusinessLogoFileHelper
                .SaveAsync(_env, api.Data.Id, logo, cancellationToken)
                .ConfigureAwait(false);

            if (logoError is not null)
            {
                _logger.LogWarning("Registration logo upload failed for {BusinessId}: {Error}", api.Data.Id, logoError);
            }
            else if (relativePath is not null)
            {
                var (profile, profileError, _) = await _profileService
                    .UpdateLogoPathAsync(api.Data.Id, relativePath, cancellationToken)
                    .ConfigureAwait(false);

                if (profileError != BusinessProfileUpdateError.None || profile is null)
                {
                    BusinessLogoFileHelper.TryDeleteStoredFile(_env, relativePath);
                    _logger.LogWarning("Registration logo path update failed for {BusinessId}", api.Data.Id);
                }
                else if (profile.LogoUrl is not null)
                {
                    api.Data.LogoUrl = ToAbsoluteUrl(profile.LogoUrl);
                }
            }
        }

        var location = $"/api/Registration/{api.Data.Id}";
        return Created(location, api);
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
