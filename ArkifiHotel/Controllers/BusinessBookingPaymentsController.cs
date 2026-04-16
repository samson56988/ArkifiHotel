using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/booking-payments")]
[Authorize(Roles = "Business")]
public sealed class BusinessBookingPaymentsController : ControllerBase
{
    private readonly IBusinessBookingPaymentService _payments;

    public BusinessBookingPaymentsController(IBusinessBookingPaymentService payments)
    {
        _payments = payments;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<BookingPaymentSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<BookingPaymentSummaryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _payments.ListAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<BookingPaymentSummaryDto>>.Ok(list));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<BookingPaymentSummaryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<BookingPaymentSummaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingPaymentRequest request, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BookingPaymentSummaryDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var created = await _payments.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (created is null)
        {
            return BadRequest(
                ApiResult<BookingPaymentSummaryDto>.Fail(
                    "Validation",
                    "Could not record payment. Check amount, booking id, status, and gateway values."));
        }

        return Created($"/api/business/booking-payments", ApiResult<BookingPaymentSummaryDto>.Ok(created));
    }
}
