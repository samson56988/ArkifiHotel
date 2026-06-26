namespace Admin.Data.Entities;

/// <summary>Branch access granted to a staff user.</summary>
public class UserOrganizationLocationPermission
{
    public Guid Id { get; set; }

    public Guid UserOrganizationId { get; set; }

    public UserOrganization UserOrganization { get; set; } = null!;

    public Guid BusinessLocationId { get; set; }

    public BusinessLocation BusinessLocation { get; set; } = null!;
}
