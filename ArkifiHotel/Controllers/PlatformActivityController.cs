using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/platform/activity")]
[Authorize(Roles = "Platform")]
public sealed class PlatformActivityController : ControllerBase
{
    private readonly IPlatformActivityService _activity;

    public PlatformActivityController(IPlatformActivityService activity)
    {
        _activity = activity;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<PagedResultDto<PlatformActivityLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListPlatformActivityQuery query,
        CancellationToken cancellationToken)
    {
        var page = await _activity.ListAsync(query, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<PagedResultDto<PlatformActivityLogDto>>.Ok(page));
    }
}
