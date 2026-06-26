using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IOrganizationAuditQueryService
{
    Task<PagedResultDto<OrganizationAuditLogDto>> ListAsync(
        Guid businessId,
        ListOrganizationAuditQuery query,
        CancellationToken cancellationToken = default);
}
