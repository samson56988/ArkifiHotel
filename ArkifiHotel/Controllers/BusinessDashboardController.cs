using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/dashboard")]
[Authorize(Roles = "Business")]
public sealed class BusinessDashboardController : ControllerBase
{
    private readonly IBusinessDashboardService _dashboard;

    public BusinessDashboardController(IBusinessDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<BusinessDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessDashboardDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BusinessDashboardDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessDashboardDto>.Fail("Unauthorized", "Missing business identity."));
        }

        if (from.HasValue != to.HasValue)
        {
            return BadRequest(ApiResult<BusinessDashboardDto>.Fail("Validation", "Provide both from and to dates, or omit both."));
        }

        if (from.HasValue && to!.Value < from.Value)
        {
            return BadRequest(ApiResult<BusinessDashboardDto>.Fail("Validation", "The to date must be on or after from."));
        }

        var dto = await _dashboard
            .GetDashboardAsync(businessId.Value, from, to, cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return NotFound(ApiResult<BusinessDashboardDto>.Fail("NotFound", "Business not found."));
        }

        return Ok(ApiResult<BusinessDashboardDto>.Ok(dto));
    }
}
