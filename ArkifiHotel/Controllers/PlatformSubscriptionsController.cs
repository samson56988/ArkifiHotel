using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/platform/subscriptions")]
[Authorize(Roles = "Platform")]
public sealed class PlatformSubscriptionsController : ControllerBase
{
    private readonly IPlatformSubscriptionAdminService _subscriptions;

    public PlatformSubscriptionsController(IPlatformSubscriptionAdminService subscriptions)
    {
        _subscriptions = subscriptions;
    }

    [HttpGet("plans")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<SubscriptionPlanDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPlans(CancellationToken cancellationToken)
    {
        var plans = await _subscriptions.ListPlansAsync(cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<SubscriptionPlanDto>>.Ok(plans));
    }

    [HttpGet("payments")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<PlatformSubscriptionPaymentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPayments(CancellationToken cancellationToken)
    {
        var payments = await _subscriptions.ListPaymentsAsync(cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<PlatformSubscriptionPaymentDto>>.Ok(payments));
    }
}
