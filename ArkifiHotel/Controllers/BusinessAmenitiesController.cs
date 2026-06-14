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
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomAmenityRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<AmenityDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _amenities.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return BadRequest(
                ApiResult<AmenityDto>.Fail(
                    "Validation",
                    "Could not create amenity. Check the name (2–128 characters) and ensure it is not a duplicate."));
        }

        return Created($"/api/business/amenities/{dto.Id}", ApiResult<AmenityDto>.Ok(dto));
    }

    [HttpPut("{amenityId:guid}")]
    [ProducesResponseType(typeof(ApiResult<AmenityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<AmenityDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<AmenityDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid amenityId,
        [FromBody] UpdateAmenityRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<AmenityDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _amenities.UpdateAsync(businessId.Value, amenityId, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            var existing = await _amenities.GetAsync(businessId.Value, amenityId, cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                return NotFound(ApiResult<AmenityDto>.Fail("NotFound", "Amenity not found."));
            }

            return BadRequest(
                ApiResult<AmenityDto>.Fail(
                    "Validation",
                    "Could not update amenity. Check the name (2–128 characters) and ensure it is not a duplicate."));
        }

        return Ok(ApiResult<AmenityDto>.Ok(dto));
    }

    [HttpDelete("{amenityId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid amenityId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var outcome = await _amenities.DeleteAsync(businessId.Value, amenityId, cancellationToken).ConfigureAwait(false);
        return outcome switch
        {
            AmenityDeleteOutcome.Deleted => Ok(ApiResult.Ok("Amenity deleted.")),
            AmenityDeleteOutcome.NotFound => NotFound(ApiResult.Fail("NotFound", "Amenity not found.")),
            AmenityDeleteOutcome.InUseByRooms => BadRequest(
                ApiResult.Fail(
                    "InUse",
                    "This amenity is assigned to one or more rooms. Remove it from those rooms before deleting.")),
            _ => BadRequest(ApiResult.Fail("Validation", "Could not delete amenity.")),
        };
    }
}
