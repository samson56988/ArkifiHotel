using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IBusinessAuthService _authService;

    public AuthController(IBusinessAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Business login — returns JWT and account summary.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<LoginBusinessData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<LoginBusinessData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<LoginBusinessData>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        var api = result.ToApiResult();

        if (api.Success)
        {
            return Ok(api);
        }

        var code = result.ErrorCode ?? "Error";
        return code switch
        {
            "InvalidCredentials" => Unauthorized(api),
            "EmailNotVerified" => Unauthorized(api),
            _ => BadRequest(api),
        };
    }

    /// <summary>Complete login 2FA by verifying OTP challenge.</summary>
    [HttpPost("verify-login-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<LoginBusinessData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<LoginBusinessData>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<LoginBusinessData>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyLoginOtp(
        [FromBody] VerifyLoginOtpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyLoginOtpAsync(request, cancellationToken);
        var api = result.ToApiResult();

        if (api.Success)
        {
            return Ok(api);
        }

        var code = result.ErrorCode ?? "Error";
        return code switch
        {
            "InvalidOtp" => Unauthorized(api),
            _ => BadRequest(api),
        };
    }

    /// <summary>Verify business email using OTP sent to inbox.</summary>
    [HttpPost("verify-email-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyEmailOtp(
        [FromBody] VerifyEmailOtpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailOtpAsync(request, cancellationToken);
        if (result.Succeeded)
        {
            return Ok(ApiResult.Ok("Email verified successfully."));
        }

        var code = result.ErrorCode ?? "Error";
        var api = ApiResult.Fail(code, result.ErrorMessage ?? "Verification failed.");
        return code switch
        {
            "NotFound" => NotFound(api),
            _ => BadRequest(api),
        };
    }
}
