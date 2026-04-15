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
            _ => BadRequest(api),
        };
    }
}
