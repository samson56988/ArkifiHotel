namespace Admin.Data.Entities;

/// <summary>Module permission granted to an organization user.</summary>
public class UserOrganizationModulePermission
{
    public Guid Id { get; set; }

    public Guid UserOrganizationId { get; set; }

    public UserOrganization UserOrganization { get; set; } = null!;

    public string ModuleCode { get; set; } = null!;
}
