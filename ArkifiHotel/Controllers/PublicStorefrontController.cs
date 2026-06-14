using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/public/stores")]
[AllowAnonymous]
public sealed class PublicStorefrontController : ControllerBase
{
    private readonly IStorefrontThemeService _themes;

    public PublicStorefrontController(IStorefrontThemeService themes)
    {
        _themes = themes;
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ApiResult<PublicStorefrontDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PublicStorefrontDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var dto = await _themes.GetPublicBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<PublicStorefrontDto>.Fail("NotFound", "Storefront not found."));
        }

        var mapped = MapPublic(dto);
        return Ok(ApiResult<PublicStorefrontDto>.Ok(mapped));
    }

    private PublicStorefrontDto MapPublic(PublicStorefrontDto dto) =>
        new()
        {
            BusinessId = dto.BusinessId,
            BusinessName = dto.BusinessName,
            Slug = dto.Slug,
            LogoUrl = ToAbsoluteUrl(dto.LogoUrl),
            Theme = dto.Theme,
            Rooms = dto.Rooms
                .Select(r => new PublicStorefrontRoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    BasePricePerNight = r.BasePricePerNight,
                    MaxOccupancy = r.MaxOccupancy,
                    PrimaryImageUrl = ToAbsoluteUrl(r.PrimaryImageUrl),
                    LocationName = r.LocationName,
                })
                .ToList(),
            Facilities = dto.Facilities
                .Select(f => new PublicStorefrontFacilityDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    PrimaryImageUrl = ToAbsoluteUrl(f.PrimaryImageUrl),
                    LocationName = f.LocationName,
                })
                .ToList(),
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
