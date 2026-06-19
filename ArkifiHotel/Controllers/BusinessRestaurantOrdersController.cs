using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/restaurant-orders")]
[Authorize(Roles = "Business")]
public sealed class BusinessRestaurantOrdersController : ControllerBase
{
    private readonly IBusinessRestaurantOrderService _orders;

    public BusinessRestaurantOrdersController(IBusinessRestaurantOrderService orders)
    {
        _orders = orders;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<RestaurantOrderListResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantOrderListResultDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var result = await _orders.ListAsync(businessId.Value, page, pageSize, status, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<RestaurantOrderListResultDto>.Ok(result));
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResult<RestaurantOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<RestaurantOrderDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<RestaurantOrderDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _orders.GetAsync(businessId.Value, orderId, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<RestaurantOrderDetailDto>.Fail("NotFound", "Order not found."));
        }

        return Ok(ApiResult<RestaurantOrderDetailDto>.Ok(dto));
    }
}
