using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/public/stores/{slug}/bookings")]
[AllowAnonymous]
public sealed class PublicGuestBookingsController : ControllerBase
{
    private readonly IPublicGuestBookingService _bookings;

    public PublicGuestBookingsController(IPublicGuestBookingService bookings)
    {
        _bookings = bookings;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<GuestBookingCheckoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<GuestBookingCheckoutDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<GuestBookingCheckoutDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCheckout(
        string slug,
        [FromBody] GuestCreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error, message) = await _bookings.CreateCheckoutAsync(slug, request, cancellationToken).ConfigureAwait(false);

        return error switch
        {
            null => Ok(ApiResult<GuestBookingCheckoutDto>.Ok(data)),
            PublicGuestBookingError.NotFound => NotFound(ApiResult<GuestBookingCheckoutDto>.Fail("NotFound", message ?? "Not found.")),
            PublicGuestBookingError.RoomUnavailable => Conflict(ApiResult<GuestBookingCheckoutDto>.Fail("RoomUnavailable", message ?? "Room unavailable.")),
            PublicGuestBookingError.PaymentNotConfigured => BadRequest(ApiResult<GuestBookingCheckoutDto>.Fail("PaymentNotConfigured", message ?? "Payment not configured.")),
            PublicGuestBookingError.PaymentInitFailed => BadRequest(ApiResult<GuestBookingCheckoutDto>.Fail("PaymentInitFailed", message ?? "Payment could not start.")),
            _ => BadRequest(ApiResult<GuestBookingCheckoutDto>.Fail("InvalidRequest", message ?? "Invalid request.")),
        };
    }

    [HttpGet("payment/verify")]
    [ProducesResponseType(typeof(ApiResult<GuestPaymentVerifyResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<GuestPaymentVerifyResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyPayment(
        string slug,
        [FromQuery] string reference,
        CancellationToken cancellationToken)
    {
        var result = await _bookings.VerifyPaymentAsync(slug, reference, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(ApiResult<GuestPaymentVerifyResultDto>.Fail("NotFound", "Storefront not found."));
        }

        return Ok(ApiResult<GuestPaymentVerifyResultDto>.Ok(result));
    }
}
