using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessTeamService
{
    Task<IReadOnlyList<OrganizationModuleDefinitionDto>> ListModuleDefinitionsAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessTeamMemberDto>> ListMembersAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<(BusinessTeamMemberDto? Data, string? ErrorCode, string? Message)> CreateMemberAsync(
        Guid businessId,
        CreateBusinessTeamMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<(BusinessTeamMemberDto? Data, string? ErrorCode, string? Message)> UpdateMemberAsync(
        Guid businessId,
        Guid memberId,
        UpdateBusinessTeamMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<(BusinessTeamMemberDto? Data, string? ErrorCode, string? Message)> SetMemberActiveStatusAsync(
        Guid businessId,
        Guid memberId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessTeamInviteDto>> ListInvitesAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<(BusinessTeamInviteDto? Data, string? ErrorCode, string? Message)> ResendInviteAsync(
        Guid businessId,
        Guid memberId,
        CancellationToken cancellationToken = default);
}
