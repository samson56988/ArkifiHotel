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
    private readonly IPublicGuestBookingService _guestBookings;

    public PublicStorefrontController(IStorefrontThemeService themes, IPublicGuestBookingService guestBookings)
    {
        _themes = themes;
        _guestBookings = guestBookings;
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ApiResult<PublicStorefrontDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PublicStorefrontDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] Guid? locationId, CancellationToken cancellationToken)
    {
        var dto = await _themes.GetPublicBySlugAsync(slug, locationId, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<PublicStorefrontDto>.Fail("NotFound", "Storefront not found."));
        }

        var mapped = MapPublic(dto);
        return Ok(ApiResult<PublicStorefrontDto>.Ok(mapped));
    }

    [HttpGet("{slug}/rooms/availability")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RoomAvailabilityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RoomAvailabilityDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RoomAvailabilityDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomAvailability(
        string slug,
        [FromQuery] Guid locationId,
        [FromQuery] DateOnly checkInDate,
        [FromQuery] DateOnly checkOutDate,
        CancellationToken cancellationToken)
    {
        var (data, error, message) = await _guestBookings
            .GetRoomAvailabilityAsync(slug, locationId, checkInDate, checkOutDate, cancellationToken)
            .ConfigureAwait(false);

        return error switch
        {
            null => Ok(ApiResult<IReadOnlyList<RoomAvailabilityDto>>.Ok(data ?? Array.Empty<RoomAvailabilityDto>())),
            PublicGuestBookingError.NotFound => NotFound(
                ApiResult<IReadOnlyList<RoomAvailabilityDto>>.Fail("NotFound", message ?? "Storefront not found.")),
            _ => BadRequest(
                ApiResult<IReadOnlyList<RoomAvailabilityDto>>.Fail("Validation", message ?? "Invalid stay dates.")),
        };
    }

    private PublicStorefrontDto MapPublic(PublicStorefrontDto dto) =>
        new()
        {
            BusinessId = dto.BusinessId,
            BusinessName = dto.BusinessName,
            Slug = dto.Slug,
            LogoUrl = ToAbsoluteUrl(dto.LogoUrl),
            Theme = dto.Theme,
            Social = dto.Social,
            Locations = dto.Locations,
            RequiresBranchSelection = dto.RequiresBranchSelection,
            ActiveLocationId = dto.ActiveLocationId,
            HeroImages = dto.HeroImages
                .Select(u => ToAbsoluteUrl(u) ?? u)
                .ToList(),
            AboutImageUrl = ToAbsoluteUrl(dto.AboutImageUrl),
            Rooms = dto.Rooms
                .Select(r => new PublicStorefrontRoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    BasePricePerNight = r.BasePricePerNight,
                    MaxOccupancy = r.MaxOccupancy,
                    PrimaryImageUrl = ToAbsoluteUrl(r.PrimaryImageUrl),
                    ImageUrls = r.ImageUrls
                        .Select(u => ToAbsoluteUrl(u) ?? u)
                        .ToList(),
                    LocationId = r.LocationId,
                    LocationName = r.LocationName,
                })
                .ToList(),
            Facilities = dto.Facilities
                .Select(f => new PublicStorefrontFacilityDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    PrimaryImageUrl = ToAbsoluteUrl(f.PrimaryImageUrl),
                    ImageUrls = f.ImageUrls
                        .Select(u => ToAbsoluteUrl(u) ?? u)
                        .ToList(),
                    LocationId = f.LocationId,
                    LocationName = f.LocationName,
                })
                .ToList(),
            Restaurant = dto.Restaurant is null
                ? null
                : new PublicStorefrontRestaurantDto
                {
                    Enabled = dto.Restaurant.Enabled,
                    NavLabel = dto.Restaurant.NavLabel,
                    HeroEyebrow = dto.Restaurant.HeroEyebrow,
                    HeroTitle = dto.Restaurant.HeroTitle,
                    HeroSubtitle = dto.Restaurant.HeroSubtitle,
                    MealsSectionTitle = dto.Restaurant.MealsSectionTitle,
                    DrinksSectionTitle = dto.Restaurant.DrinksSectionTitle,
                    HeroImageUrl = ToAbsoluteUrl(dto.Restaurant.HeroImageUrl),
                    FoodCategories = MapMenuCategories(dto.Restaurant.FoodCategories),
                    DrinkCategories = MapMenuCategories(dto.Restaurant.DrinkCategories),
                },
        };

    private IReadOnlyList<PublicStorefrontMenuCategoryDto> MapMenuCategories(
        IReadOnlyList<PublicStorefrontMenuCategoryDto> categories) =>
        categories
            .Select(c => new PublicStorefrontMenuCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Items = c.Items
                    .Select(i => new PublicStorefrontMenuItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Description = i.Description,
                        Price = i.Price,
                        Tags = i.Tags,
                        ImageUrl = ToAbsoluteUrl(i.ImageUrl),
                    })
                    .ToList(),
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
