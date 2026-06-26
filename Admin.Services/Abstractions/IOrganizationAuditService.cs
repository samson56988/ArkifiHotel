namespace Admin.Services.Abstractions;

public sealed record OrganizationAuditEntry(
    string Action,
    string EntityType,
    Guid? EntityId,
    Guid? LocationId,
    string? LocationName,
    string Summary,
    string? DetailsJson = null);

public interface IOrganizationAuditService
{
    Task LogAsync(
        Guid businessId,
        OrganizationAuditEntry entry,
        Guid? userOrganizationId = null,
        string? userDisplayName = null,
        string? userEmail = null,
        CancellationToken cancellationToken = default);

    Task LogForCurrentUserAsync(
        Guid businessId,
        OrganizationAuditEntry entry,
        CancellationToken cancellationToken = default);
}
