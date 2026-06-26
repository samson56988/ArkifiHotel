using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/audit")]
[Authorize(Roles = "Business")]
public sealed class BusinessAuditController : ControllerBase
{
    private readonly IOrganizationAuditQueryService _audit;

    public BusinessAuditController(IOrganizationAuditQueryService audit)
    {
        _audit = audit;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<PagedResultDto<OrganizationAuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListOrganizationAuditQuery query, CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<PagedResultDto<OrganizationAuditLogDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var page = await _audit.ListAsync(businessId.Value, query, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<PagedResultDto<OrganizationAuditLogDto>>.Ok(page));
    }
}
