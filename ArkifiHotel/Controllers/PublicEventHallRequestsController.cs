using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/public/stores/{slug}/event-hall-requests")]
[AllowAnonymous]
public sealed class PublicEventHallRequestsController : ControllerBase
{
    private readonly IPublicEventHallRequestService _requests;

    public PublicEventHallRequestsController(IPublicEventHallRequestService requests)
    {
        _requests = requests;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<GuestEventHallRequestResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<GuestEventHallRequestResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<GuestEventHallRequestResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        string slug,
        [FromBody] GuestCreateEventHallRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error) = await _requests.CreateRequestAsync(slug, request, cancellationToken).ConfigureAwait(false);

        if (data is null)
        {
            var message = error ?? "Could not submit request.";
            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResult<GuestEventHallRequestResultDto>.Fail("NotFound", message));
            }

            return BadRequest(ApiResult<GuestEventHallRequestResultDto>.Fail("InvalidRequest", message));
        }

        return Ok(ApiResult<GuestEventHallRequestResultDto>.Ok(data));
    }
}
