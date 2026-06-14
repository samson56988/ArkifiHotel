using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/locations")]
[Authorize(Roles = "Business")]
public sealed class BusinessLocationsController : ControllerBase
{
    private readonly IBusinessLocationService _locations;

    public BusinessLocationsController(IBusinessLocationService locations)
    {
        _locations = locations;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<BusinessLocationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<BusinessLocationDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _locations.ListAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<BusinessLocationDto>>.Ok(list));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<BusinessLocationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<BusinessLocationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBusinessLocationRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessLocationDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _locations.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return BadRequest(
                ApiResult<BusinessLocationDto>.Fail(
                    "Validation",
                    "Could not create location. Check the name (2–200 characters) and ensure it is not a duplicate."));
        }

        return Created($"/api/business/locations/{dto.Id}", ApiResult<BusinessLocationDto>.Ok(dto));
    }

    [HttpPut("{locationId:guid}")]
    [ProducesResponseType(typeof(ApiResult<BusinessLocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessLocationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BusinessLocationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid locationId,
        [FromBody] UpdateBusinessLocationRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessLocationDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _locations.UpdateAsync(businessId.Value, locationId, request, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            var existing = await _locations.GetAsync(businessId.Value, locationId, cancellationToken).ConfigureAwait(false);
            if (existing is null)
            {
                return NotFound(ApiResult<BusinessLocationDto>.Fail("NotFound", "Location not found."));
            }

            return BadRequest(
                ApiResult<BusinessLocationDto>.Fail(
                    "Validation",
                    "Could not update location. Check the name (2–200 characters) and ensure it is not a duplicate."));
        }

        return Ok(ApiResult<BusinessLocationDto>.Ok(dto));
    }

    [HttpDelete("{locationId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid locationId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var outcome = await _locations.DeleteAsync(businessId.Value, locationId, cancellationToken).ConfigureAwait(false);
        return outcome switch
        {
            LocationDeleteOutcome.Deleted => Ok(ApiResult.Ok("Location deleted.")),
            LocationDeleteOutcome.NotFound => NotFound(ApiResult.Fail("NotFound", "Location not found.")),
            LocationDeleteOutcome.InUse => BadRequest(
                ApiResult.Fail(
                    "InUse",
                    "This location is assigned to one or more rooms or facilities. Reassign or clear them before deleting.")),
            _ => BadRequest(ApiResult.Fail("Validation", "Could not delete location.")),
        };
    }
}
