using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/public/stores/{slug}/restaurant-orders")]
[AllowAnonymous]
public sealed class PublicRestaurantOrdersController : ControllerBase
{
    private readonly IPublicRestaurantOrderService _orders;

    public PublicRestaurantOrdersController(IPublicRestaurantOrderService orders)
    {
        _orders = orders;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<GuestRestaurantOrderCheckoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<GuestRestaurantOrderCheckoutDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<GuestRestaurantOrderCheckoutDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCheckout(
        string slug,
        [FromBody] GuestCreateRestaurantOrderRequest request,
        CancellationToken cancellationToken)
    {
        var (data, error, message) = await _orders.CreateCheckoutAsync(slug, request, cancellationToken).ConfigureAwait(false);

        return error switch
        {
            null => Ok(ApiResult<GuestRestaurantOrderCheckoutDto>.Ok(data)),
            PublicRestaurantOrderError.NotFound => NotFound(ApiResult<GuestRestaurantOrderCheckoutDto>.Fail("NotFound", message ?? "Not found.")),
            PublicRestaurantOrderError.ItemUnavailable => Conflict(ApiResult<GuestRestaurantOrderCheckoutDto>.Fail("ItemUnavailable", message ?? "Item unavailable.")),
            PublicRestaurantOrderError.PaymentNotConfigured => BadRequest(ApiResult<GuestRestaurantOrderCheckoutDto>.Fail("PaymentNotConfigured", message ?? "Payment not configured.")),
            PublicRestaurantOrderError.PaymentInitFailed => BadRequest(ApiResult<GuestRestaurantOrderCheckoutDto>.Fail("PaymentInitFailed", message ?? "Payment could not start.")),
            _ => BadRequest(ApiResult<GuestRestaurantOrderCheckoutDto>.Fail("InvalidRequest", message ?? "Invalid request.")),
        };
    }

    [HttpGet("payment/verify")]
    [ProducesResponseType(typeof(ApiResult<GuestRestaurantOrderVerifyResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<GuestRestaurantOrderVerifyResultDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyPayment(
        string slug,
        [FromQuery] string reference,
        CancellationToken cancellationToken)
    {
        var result = await _orders.VerifyPaymentAsync(slug, reference, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(ApiResult<GuestRestaurantOrderVerifyResultDto>.Fail("NotFound", "Storefront not found."));
        }

        return Ok(ApiResult<GuestRestaurantOrderVerifyResultDto>.Ok(result));
    }
}
