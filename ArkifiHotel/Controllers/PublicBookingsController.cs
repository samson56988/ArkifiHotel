using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/public/bookings")]
[AllowAnonymous]
public sealed class PublicBookingsController : ControllerBase
{
    private readonly IPublicBookingLookupService _lookup;

    public PublicBookingsController(IPublicBookingLookupService lookup)
    {
        _lookup = lookup;
    }

    /// <summary>Look up a booking by confirmation code (no login).</summary>
    [HttpGet("{confirmationCode}")]
    [ProducesResponseType(typeof(ApiResult<GuestBookingLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<GuestBookingLookupDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string confirmationCode, CancellationToken cancellationToken)
    {
        var dto = await _lookup.GetByConfirmationCodeAsync(confirmationCode, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<GuestBookingLookupDto>.Fail("NotFound", "No booking matches that confirmation code."));
        }

        return Ok(ApiResult<GuestBookingLookupDto>.Ok(dto));
    }
}
