using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/platform/businesses")]
[Authorize(Roles = "Platform")]
public sealed class PlatformBusinessesController : ControllerBase
{
    private readonly IPlatformBusinessService _businesses;

    public PlatformBusinessesController(IPlatformBusinessService businesses)
    {
        _businesses = businesses;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResult<PlatformDashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var stats = await _businesses.GetDashboardStatsAsync(cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<PlatformDashboardStatsDto>.Ok(stats));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<PlatformBusinessSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await _businesses.ListAsync(cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<PlatformBusinessSummaryDto>>.Ok(items));
    }

    [HttpGet("{businessId:guid}")]
    [ProducesResponseType(typeof(ApiResult<PlatformBusinessDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PlatformBusinessDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid businessId, CancellationToken cancellationToken)
    {
        var detail = await _businesses.GetByIdAsync(businessId, cancellationToken).ConfigureAwait(false);
        if (detail is null)
        {
            return NotFound(ApiResult<PlatformBusinessDetailDto>.Fail("NotFound", "Business not found."));
        }

        return Ok(ApiResult<PlatformBusinessDetailDto>.Ok(detail));
    }

    [HttpPatch("{businessId:guid}")]
    [ProducesResponseType(typeof(ApiResult<PlatformBusinessDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PlatformBusinessDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<PlatformBusinessDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid businessId,
        [FromBody] UpdatePlatformBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await _businesses.UpdateAsync(businessId, request, cancellationToken)
            .ConfigureAwait(false);

        if (error == "Business not found.")
        {
            return NotFound(ApiResult<PlatformBusinessDetailDto>.Fail("NotFound", error));
        }

        if (error is not null)
        {
            return BadRequest(ApiResult<PlatformBusinessDetailDto>.Fail("Validation", error));
        }

        return Ok(ApiResult<PlatformBusinessDetailDto>.Ok(data!));
    }
}
