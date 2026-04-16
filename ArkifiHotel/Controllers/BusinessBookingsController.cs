using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/bookings")]
[Authorize(Roles = "Business")]
public sealed class BusinessBookingsController : ControllerBase
{
    private readonly IBusinessBookingService _bookings;

    public BusinessBookingsController(IBusinessBookingService bookings)
    {
        _bookings = bookings;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<BookingSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<BookingSummaryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _bookings.ListAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<BookingSummaryDto>>.Ok(list));
    }

    [HttpGet("{bookingId:guid}")]
    [ProducesResponseType(typeof(ApiResult<BookingDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BookingDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid bookingId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BookingDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var booking = await _bookings.GetAsync(businessId.Value, bookingId, cancellationToken).ConfigureAwait(false);
        if (booking is null)
        {
            return NotFound(ApiResult<BookingDetailDto>.Fail("NotFound", "Booking not found."));
        }

        return Ok(ApiResult<BookingDetailDto>.Ok(booking));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<BookingDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<BookingDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BookingDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var created = await _bookings.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (created is null)
        {
            return BadRequest(
                ApiResult<BookingDetailDto>.Fail(
                    "Validation",
                    "Could not create booking. Check guest details, dates, room selection, and that the room is active with no overlapping reservation."));
        }

        return Created($"/api/business/bookings/{created.Id}", ApiResult<BookingDetailDto>.Ok(created));
    }

    [HttpPatch("{bookingId:guid}/status")]
    [ProducesResponseType(typeof(ApiResult<BookingDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BookingDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BookingDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid bookingId,
        [FromBody] UpdateBookingStatusRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BookingDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var updated = await _bookings.UpdateStatusAsync(businessId.Value, bookingId, request.Status, cancellationToken).ConfigureAwait(false);
        if (updated is null)
        {
            var exists = await _bookings.GetAsync(businessId.Value, bookingId, cancellationToken).ConfigureAwait(false);
            if (exists is null)
            {
                return NotFound(ApiResult<BookingDetailDto>.Fail("NotFound", "Booking not found."));
            }

            return BadRequest(ApiResult<BookingDetailDto>.Fail("Validation", "Invalid status. Use Pending, Confirmed, Cancelled, or Completed."));
        }

        return Ok(ApiResult<BookingDetailDto>.Ok(updated));
    }
}
