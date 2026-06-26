using Admin.Services.Abstractions;
using ArkifiHotel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Data.Api;
using Shared.Data.Dtos;

namespace ArkifiHotel.Controllers;

[ApiController]
[Route("api/business/team")]
[Authorize(Roles = "Business")]
public sealed class BusinessTeamController : ControllerBase
{
    private readonly IBusinessTeamService _team;

    public BusinessTeamController(IBusinessTeamService team)
    {
        _team = team;
    }

    [HttpGet("modules")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<OrganizationModuleDefinitionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListModules(CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<OrganizationModuleDefinitionDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _team.ListModuleDefinitionsAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<OrganizationModuleDefinitionDto>>.Ok(list));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<BusinessTeamMemberDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMembers(CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<BusinessTeamMemberDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _team.ListMembersAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<BusinessTeamMemberDto>>.Ok(list));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamMemberDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamMemberDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMember(
        [FromBody] CreateBusinessTeamMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessTeamMemberDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, errorCode, message) = await _team
            .CreateMemberAsync(businessId.Value, request, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
        {
            return errorCode switch
            {
                "DuplicateEmail" or "DuplicateUsername" => Conflict(ApiResult<BusinessTeamMemberDto>.Fail(errorCode!, message ?? "Conflict.")),
                "NotFound" => NotFound(ApiResult<BusinessTeamMemberDto>.Fail(errorCode!, message ?? "Not found.")),
                _ => BadRequest(ApiResult<BusinessTeamMemberDto>.Fail(errorCode ?? "Validation", message ?? "Invalid request.")),
            };
        }

        return Created($"/api/business/team/{data.Id}", ApiResult<BusinessTeamMemberDto>.Ok(data));
    }

    [HttpPut("{memberId:guid}")]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamMemberDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMember(
        Guid memberId,
        [FromBody] UpdateBusinessTeamMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessTeamMemberDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, errorCode, message) = await _team
            .UpdateMemberAsync(businessId.Value, memberId, request, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
        {
            return errorCode switch
            {
                "DuplicateEmail" => Conflict(ApiResult<BusinessTeamMemberDto>.Fail(errorCode!, message ?? "Conflict.")),
                "NotFound" => NotFound(ApiResult<BusinessTeamMemberDto>.Fail(errorCode!, message ?? "Not found.")),
                _ => BadRequest(ApiResult<BusinessTeamMemberDto>.Fail(errorCode ?? "Validation", message ?? "Invalid request.")),
            };
        }

        return Ok(ApiResult<BusinessTeamMemberDto>.Ok(data));
    }

    [HttpPatch("{memberId:guid}/status")]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamMemberDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetMemberStatus(
        Guid memberId,
        [FromBody] SetBusinessTeamMemberStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessTeamMemberDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, errorCode, message) = await _team
            .SetMemberActiveStatusAsync(businessId.Value, memberId, request.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
        {
            return errorCode switch
            {
                "NotFound" => NotFound(ApiResult<BusinessTeamMemberDto>.Fail(errorCode!, message ?? "Not found.")),
                _ => BadRequest(ApiResult<BusinessTeamMemberDto>.Fail(errorCode ?? "Validation", message ?? "Invalid request.")),
            };
        }

        return Ok(ApiResult<BusinessTeamMemberDto>.Ok(data));
    }

    [HttpGet("invites")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<BusinessTeamInviteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListInvites(CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<IReadOnlyList<BusinessTeamInviteDto>>.Fail("Unauthorized", "Missing business identity."));
        }

        var list = await _team.ListInvitesAsync(businessId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(ApiResult<IReadOnlyList<BusinessTeamInviteDto>>.Ok(list));
    }

    [HttpPost("{memberId:guid}/resend-invite")]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamInviteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamInviteDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult<BusinessTeamInviteDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendInvite(Guid memberId, CancellationToken cancellationToken)
    {
        if (!User.IsSuperAdmin())
        {
            return Forbid();
        }

        var businessId = User.GetBusinessId();
        if (businessId is null)
        {
            return Unauthorized(ApiResult<BusinessTeamInviteDto>.Fail("Unauthorized", "Missing business identity."));
        }

        var (data, errorCode, message) = await _team
            .ResendInviteAsync(businessId.Value, memberId, cancellationToken)
            .ConfigureAwait(false);

        if (data is null)
        {
            return errorCode switch
            {
                "NotFound" => NotFound(ApiResult<BusinessTeamInviteDto>.Fail(errorCode!, message ?? "Not found.")),
                _ => BadRequest(ApiResult<BusinessTeamInviteDto>.Fail(errorCode ?? "Validation", message ?? "Invalid request.")),
            };
        }

        return Ok(ApiResult<BusinessTeamInviteDto>.Ok(data, "Invite email resent."));
    }
}
