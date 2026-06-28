using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IBusinessAuthService _authService;
    private readonly IBusinessPasswordResetService _passwordResetService;
    private readonly IBusinessTokenRevocationService _tokenRevocation;

    public AuthController(
        IBusinessAuthService authService,
        IBusinessPasswordResetService passwordResetService,
        IBusinessTokenRevocationService tokenRevocation)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
        _tokenRevocation = tokenRevocation;
    }

    /// <summary>Sign out — revokes the current JWT so it cannot be reused.</summary>
    [HttpPost("logout")]
    [Authorize(Roles = "Business")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var businessId = User.GetBusinessId();
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (businessId is null || string.IsNullOrWhiteSpace(jti))
        {
            return BadRequest(ApiResult.Fail("Validation", "Could not identify the active session."));
        }

        var expiresAtUtc = ResolveTokenExpiryUtc(User);
        await _tokenRevocation
            .RevokeAsync(jti, businessId.Value, User.GetUserId(), expiresAtUtc, cancellationToken)
            .ConfigureAwait(false);

        return Ok(ApiResult.Ok("Signed out."));
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
            "AccountBlocked" => StatusCode(StatusCodes.Status403Forbidden, api),
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
            "AccountBlocked" => StatusCode(StatusCodes.Status403Forbidden, api),
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

    /// <summary>Replace a temporary staff password after first sign-in.</summary>
    [HttpPost("change-default-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeDefaultPassword(
        [FromBody] ChangeDefaultPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ChangeDefaultPasswordAsync(request, cancellationToken);
        if (result.Success)
        {
            return Ok(ApiResult.Ok("Password updated. You can sign in with your new password."));
        }

        var api = ApiResult.Fail(result.ErrorCode ?? "Error", result.Message ?? "Could not update password.");
        return (result.ErrorCode ?? string.Empty) switch
        {
            "InvalidCredentials" => Unauthorized(api),
            "AccountBlocked" => StatusCode(StatusCodes.Status403Forbidden, api),
            _ => BadRequest(api),
        };
    }

    private static DateTimeOffset ResolveTokenExpiryUtc(System.Security.Claims.ClaimsPrincipal user)
    {
        var exp = user.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (long.TryParse(exp, out var unix))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix);
        }

        return DateTimeOffset.UtcNow.AddHours(1);
    }
}
