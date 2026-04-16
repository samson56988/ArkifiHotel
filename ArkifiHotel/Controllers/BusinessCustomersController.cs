using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/customers")]
[Authorize(Roles = "Business")]
public sealed class BusinessCustomersController : ControllerBase
{
    private readonly IBusinessCustomerService _customers;

    public BusinessCustomersController(IBusinessCustomerService customers)
    {
        _customers = customers;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<CustomerSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<CustomerSummaryDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _customers.ListAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<CustomerSummaryDto>>.Ok(list));
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResult<CustomerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<CustomerDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid customerId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<CustomerDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _customers.GetAsync(businessId.Value, customerId, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            return NotFound(ApiResult<CustomerDetailDto>.Fail("NotFound", "Customer not found."));
        }

        return Ok(ApiResult<CustomerDetailDto>.Ok(dto));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<CustomerDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<CustomerDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<CustomerDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var created = await _customers.CreateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (created is null)
        {
            return BadRequest(
                ApiResult<CustomerDetailDto>.Fail(
                    "Validation",
                    "Could not create customer. Check name, email, and that the email is not already used."));
        }

        return Created($"/api/business/customers/{created.Id}", ApiResult<CustomerDetailDto>.Ok(created));
    }

    [HttpPut("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResult<CustomerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<CustomerDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<CustomerDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<CustomerDetailDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var updated = await _customers.UpdateAsync(businessId.Value, customerId, request, cancellationToken).ConfigureAwait(false);
        if (updated is null)
        {
            var exists = await _customers.GetAsync(businessId.Value, customerId, cancellationToken).ConfigureAwait(false);
            if (exists is null)
            {
                return NotFound(ApiResult<CustomerDetailDto>.Fail("NotFound", "Customer not found."));
            }

            return BadRequest(
                ApiResult<CustomerDetailDto>.Fail(
                    "Validation",
                    "Could not update customer. Check fields and that the email is not already used."));
        }

        return Ok(ApiResult<CustomerDetailDto>.Ok(updated));
    }

    [HttpDelete("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid customerId, CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult.Fail("Unauthorized", "Missing business identity."));
        }

        var ok = await _customers.DeleteAsync(businessId.Value, customerId, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return NotFound(ApiResult.Fail("NotFound", "Customer not found."));
        }

        return Ok(ApiResult.Ok("Customer removed."));
    }
}
