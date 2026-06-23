using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/public/subscription-plans")]
[AllowAnonymous]
public sealed class PublicSubscriptionPlansController : ControllerBase
{
    private readonly ISubscriptionPlanService _plans;

    public PublicSubscriptionPlansController(ISubscriptionPlanService plans)
    {
        _plans = plans;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<SubscriptionPlanDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var plans = await _plans.GetActivePlansAsync(cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<SubscriptionPlanDto>>.Ok(plans));
    }
}
