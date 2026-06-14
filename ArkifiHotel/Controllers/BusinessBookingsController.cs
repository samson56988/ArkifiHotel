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
    [ProducesResponseType(typeof(ApiResult<PagedResultDto<BookingSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PagedResultDto<BookingSummaryDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateOnly? checkInFrom = null,
        [FromQuery] DateOnly? checkInTo = null,
        [FromQuery] DateOnly? checkOutFrom = null,
        [FromQuery] DateOnly? checkOutTo = null,
        [FromQuery] string? stayPhase = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PagedResultDto<BookingSummaryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        if (checkInFrom.HasValue && checkInTo.HasValue && checkInTo.Value < checkInFrom.Value)
        {
            return BadRequest(
                ApiResult<PagedResultDto<BookingSummaryDto>>.Fail("Validation", "Check-in “to” must be on or after check-in “from”."));
        }

        if (checkOutFrom.HasValue && checkOutTo.HasValue && checkOutTo.Value < checkOutFrom.Value)
        {
            return BadRequest(
                ApiResult<PagedResultDto<BookingSummaryDto>>.Fail("Validation", "Check-out “to” must be on or after check-out “from”."));
        }

        var query = new ListBookingsQuery
        {
            Page = page,
            PageSize = pageSize,
            CheckInFrom = checkInFrom,
            CheckInTo = checkInTo,
            CheckOutFrom = checkOutFrom,
            CheckOutTo = checkOutTo,
            StayPhase = stayPhase,
            Status = status,
        };

        var list = await _bookings.ListAsync(businessId.Value, query, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<PagedResultDto<BookingSummaryDto>>.Ok(list));
    }

    [HttpGet("availability")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RoomAvailabilityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<RoomAvailabilityDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] DateOnly checkInDate,
        [FromQuery] DateOnly checkOutDate,
        [FromQuery] Guid? roomId = null,
        [FromQuery] Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<RoomAvailabilityDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        if (checkOutDate <= checkInDate)
        {
            return BadRequest(
                ApiResult<IReadOnlyList<RoomAvailabilityDto>>.Fail(
                    "Validation",
                    "Check-out must be after check-in."));
        }

        var availability = await _bookings
            .GetAvailabilityAsync(businessId.Value, checkInDate, checkOutDate, roomId, locationId, cancellationToken)
            .ConfigureAwait(false);

        return Ok(ApiResult<IReadOnlyList<RoomAvailabilityDto>>.Ok(availability));
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
                    "Could not create booking. Select a location, check guest details (including phone), dates, room availability, and confirm payment (cash or bank transfer only)."));
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
