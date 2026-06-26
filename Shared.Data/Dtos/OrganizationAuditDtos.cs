namespace Shared.Data.Dtos;

public sealed class OrganizationAuditLogDto
{
    public Guid Id { get; set; }

    public Guid? UserOrganizationId { get; set; }

    public string? UserDisplayName { get; set; }

    public string? UserEmail { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }

    public string? Summary { get; set; }

    public string? DetailsJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ListOrganizationAuditQuery
{
    public Guid? LocationId { get; set; }

    public string? EntityType { get; set; }

    public string? Action { get; set; }

    public Guid? UserOrganizationId { get; set; }

    public DateTimeOffset? FromUtc { get; set; }

    public DateTimeOffset? ToUtc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}
