namespace Admin.Services.Abstractions;

public interface IBusinessTokenRevocationService
{
    Task RevokeAsync(
        string jti,
        Guid businessRegistrationId,
        Guid? userOrganizationId,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken = default);

    Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default);
}
