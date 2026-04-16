using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/payment-configuration")]
[Authorize(Roles = "Business")]
public sealed class BusinessPaymentConfigurationController : ControllerBase
{
    private readonly IBusinessPaymentConfigurationService _payment;

    public BusinessPaymentConfigurationController(IBusinessPaymentConfigurationService payment)
    {
        _payment = payment;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<PaymentConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PaymentConfigurationDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _payment.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<PaymentConfigurationDto>.Fail("NotFound", "Business not found."));
        }

        return Ok(ApiResult<PaymentConfigurationDto>.Ok(dto));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResult<PaymentConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PaymentConfigurationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<PaymentConfigurationDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromBody] UpdatePaymentConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PaymentConfigurationDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, error) = await _payment.UpdateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (error == PaymentConfigurationUpdateError.NotFound)
        {
            return NotFound(ApiResult<PaymentConfigurationDto>.Fail("NotFound", "Business not found."));
        }

        if (error == PaymentConfigurationUpdateError.InvalidRequest)
        {
            return BadRequest(
                ApiResult<PaymentConfigurationDto>.Fail(
                    "Validation",
                    "Invalid provider or secret. Use None, Paystack, or Flutterwave. When enabling or switching provider, provide a secret key (8–512 characters). Leave secret empty only to keep the existing key when the provider is unchanged."));
        }

        return Ok(ApiResult<PaymentConfigurationDto>.Ok(data!));
    }
}
