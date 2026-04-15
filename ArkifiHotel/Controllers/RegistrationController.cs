using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IBusinessRegistrationService _registrationService;

    public RegistrationController(IBusinessRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    /// <summary>Register a new business (hotel / shortlet) account.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<BusinessRegistrationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<BusinessRegistrationDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BusinessRegistrationDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registrationService.RegisterAsync(request, cancellationToken);
        var api = result.ToApiResult();

        if (api.Success && api.Data is not null)
        {
            var location = $"/api/Registration/{api.Data.Id}";
            return Created(location, api);
        }

        var code = result.ErrorCode ?? "Error";
        return code switch
        {
            "DuplicateEmail" => Conflict(api),
            _ => BadRequest(api),
        };
    }
}
