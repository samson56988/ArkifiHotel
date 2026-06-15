using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/social-profile")]
[Authorize(Roles = "Business")]
public sealed class BusinessSocialProfileController : ControllerBase
{
    private readonly IBusinessSocialProfileService _social;

    public BusinessSocialProfileController(IBusinessSocialProfileService social)
    {
        _social = social;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<BusinessSocialProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessSocialProfileDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var dto = await _social.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<BusinessSocialProfileDto>.Ok(dto));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResult<BusinessSocialProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessSocialProfileDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromBody] UpdateBusinessSocialProfileRequest request,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessSocialProfileDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, error) = await _social.UpdateAsync(businessId.Value, request, cancellationToken).ConfigureAwait(false);
        if (data is null)
        {
            return BadRequest(ApiResult<BusinessSocialProfileDto>.Fail("Validation", error ?? "Could not save social profile."));
        }

        return Ok(ApiResult<BusinessSocialProfileDto>.Ok(data));
    }
}
