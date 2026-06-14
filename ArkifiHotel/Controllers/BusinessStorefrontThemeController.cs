using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/storefront-theme")]
[Authorize(Roles = "Business")]
public sealed class BusinessStorefrontThemeController : ControllerBase
{
    private readonly IStorefrontThemeService _themes;

    public BusinessStorefrontThemeController(IStorefrontThemeService themes)
    {
        _themes = themes;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<StorefrontThemeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<StorefrontThemeDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var theme = await _themes.GetAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        if (theme is null)
        {
            return NotFound(ApiResult<StorefrontThemeDto>.Fail("NotFound", "Business not found."));
        }

        return Ok(ApiResult<StorefrontThemeDto>.Ok(theme));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResult<StorefrontThemeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<StorefrontThemeDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromBody] StorefrontThemeDto theme,
        CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<StorefrontThemeDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, error) = await _themes.UpdateAsync(businessId.Value, theme, cancellationToken).ConfigureAwait(false);
        if (data is null)
        {
            return BadRequest(ApiResult<StorefrontThemeDto>.Fail("Validation", error ?? "Could not save theme."));
        }

        return Ok(ApiResult<StorefrontThemeDto>.Ok(data));
    }
}
