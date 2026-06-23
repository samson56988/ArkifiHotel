using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/subscription")]
[Authorize(Roles = "Business")]
public sealed class BusinessSubscriptionController : ControllerBase
{
    private readonly IBusinessSubscriptionService _subscription;

    public BusinessSubscriptionController(IBusinessSubscriptionService subscription)
    {
        _subscription = subscription;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<BusinessSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessSubscriptionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessSubscriptionDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _subscription.GetCurrentAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<BusinessSubscriptionDto>.Fail("NotFound", "Business not found."));
        }

        return Ok(ApiResult<BusinessSubscriptionDto>.Ok(dto));
    }

    [HttpGet("plans")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<SubscriptionPlanOptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPlanOptions(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<SubscriptionPlanOptionDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var plans = await _subscription.GetPlanOptionsAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<SubscriptionPlanOptionDto>>.Ok(plans));
    }

    [HttpPost("initialize-payment")]
    [ProducesResponseType(typeof(ApiResult<InitSubscriptionPaymentResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<InitSubscriptionPaymentResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitializePayment(
        [FromBody] InitSubscriptionPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<InitSubscriptionPaymentResultDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, error) = await _subscription
            .InitializePaymentAsync(businessId.Value, request.PlanCode, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
        {
            return BadRequest(ApiResult<InitSubscriptionPaymentResultDto>.Fail("Validation", error ?? "Could not start payment."));
        }

        return Ok(ApiResult<InitSubscriptionPaymentResultDto>.Ok(data));
    }

    [HttpPost("verify-payment")]
    [ProducesResponseType(typeof(ApiResult<BusinessSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessSubscriptionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyPayment(
        [FromQuery] string reference,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessSubscriptionDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, error) = await _subscription
            .VerifyPaymentAsync(businessId.Value, reference, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
        {
            return BadRequest(ApiResult<BusinessSubscriptionDto>.Fail("Validation", error ?? "Payment verification failed."));
        }

        return Ok(ApiResult<BusinessSubscriptionDto>.Ok(data));
    }

    [HttpPost("change-plan")]
    [ProducesResponseType(typeof(ApiResult<BusinessSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessSubscriptionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePlan(
        [FromBody] ChangeSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessSubscriptionDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, error) = await _subscription
            .ChangePlanWithoutPaymentAsync(businessId.Value, request.PlanCode, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
        {
            return BadRequest(ApiResult<BusinessSubscriptionDto>.Fail("Validation", error ?? "Could not change plan."));
        }

        return Ok(ApiResult<BusinessSubscriptionDto>.Ok(data));
    }

    [HttpGet("payments")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<BusinessSubscriptionPaymentHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPayments(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<BusinessSubscriptionPaymentHistoryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var payments = await _subscription.GetPaymentHistoryAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<BusinessSubscriptionPaymentHistoryDto>>.Ok(payments));
    }
}
