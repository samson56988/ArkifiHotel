using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/platform/auth")]
public sealed class PlatformAuthController : ControllerBase
{
    private readonly IPlatformAuthService _auth;

    public PlatformAuthController(IPlatformAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<PlatformLoginData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<PlatformLoginData>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] PlatformLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _auth.LoginAsync(request, cancellationToken).ConfigureAwait(false);
        if (result.Success)
        {
            return Ok(ApiResult<PlatformLoginData>.Ok(result.Data!));
        }

        var api = ApiResult<PlatformLoginData>.Fail(result.ErrorCode ?? "Error", result.Message ?? "Login failed.");
        return result.ErrorCode == "InvalidCredentials"
            ? Unauthorized(api)
            : BadRequest(api);
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Platform")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public IActionResult Logout() => Ok(ApiResult.Ok("Signed out."));
}
