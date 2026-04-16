using System.Security.Claims;
using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/amenities")]
[Authorize(Roles = "Business")]
public sealed class BusinessAmenitiesController : ControllerBase
{
    private readonly IBusinessAmenityService _amenities;

    public BusinessAmenitiesController(IBusinessAmenityService amenities)
    {
        _amenities = amenities;
    }

    /// <summary>Catalog amenities plus this business's custom amenities.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<AmenityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<AmenityDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<AmenityDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _amenities.ListForBusinessAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<AmenityDto>>.Ok(list));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<AmenityDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<AmenityDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustom(
        [FromBody] CreateCustomAmenityRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<AmenityDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _amenities.CreateCustomAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return BadRequest(
                ApiResult<AmenityDto>.Fail(
                    "Validation",
                    "Could not create amenity. Check the name (2–128 characters) and ensure it is not a duplicate."));
        }

        return Created($"/api/business/amenities/{dto.Id}", ApiResult<AmenityDto>.Ok(dto));
    }
}
