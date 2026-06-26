namespace Admin.Data.Entities;

/// <summary>Persisted audit trail for business admin actions.</summary>
public class OrganizationAuditLog
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid? UserOrganizationId { get; set; }

    public string? UserDisplayName { get; set; }

    public string? UserEmail { get; set; }

    public string Action { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public Guid? EntityId { get; set; }

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }

    public string? Summary { get; set; }

    public string? DetailsJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
