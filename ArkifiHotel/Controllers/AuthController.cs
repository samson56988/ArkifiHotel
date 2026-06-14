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
    private readonly IBusinessPasswordResetService _passwordResetService;

    public AuthController(
        IBusinessAuthService authService,
        IBusinessPasswordResetService passwordResetService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
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
        var verifyApi = result.ToApiResult();
        if (verifyApi.Success)
        {
            return Ok(verifyApi);
        }

        var code = result.ErrorCode ?? "Error";
        return code switch
        {
            "NotFound" => NotFound(verifyApi),
            _ => BadRequest(verifyApi),
        };
    }

    /// <summary>Request a password reset OTP (always returns a generic message when email is unknown).</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<RequestPasswordResetData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<RequestPasswordResetData>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _passwordResetService.RequestResetAsync(request, cancellationToken);
        var api = result.ToApiResult();

        if (api.Success)
        {
            return Ok(api);
        }

        return BadRequest(api);
    }

    /// <summary>Verify reset OTP and set a new password.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _passwordResetService.ResetPasswordAsync(request, cancellationToken);
        var resetApi = result.ToApiResult();
        if (resetApi.Success)
        {
            return Ok(resetApi);
        }

        return (result.ErrorCode ?? string.Empty) switch
        {
            "InvalidOtp" => Unauthorized(resetApi),
            _ => BadRequest(resetApi),
        };
    }
}
