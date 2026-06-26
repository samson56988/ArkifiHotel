namespace Admin.Services.Abstractions;

public interface IOrganizationUserContext
{
    Guid? BusinessId { get; }

    Guid? UserOrganizationId { get; }

    bool IsSuperAdmin { get; }

    bool HasAllLocationAccess { get; }

    IReadOnlyList<Guid> LocationIds { get; }

    bool CanAccessLocation(Guid? locationId);
}
