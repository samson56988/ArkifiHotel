using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Services;

public sealed class BusinessTokenRevocationService : IBusinessTokenRevocationService
{
    private readonly AdminDbContext _db;

    public BusinessTokenRevocationService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task RevokeAsync(
        string jti,
        Guid businessRegistrationId,
        Guid? userOrganizationId,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        var normalizedJti = jti.Trim();
        var exists = await _db.RevokedBusinessAccessTokens
            .AsNoTracking()
            .AnyAsync(t => t.Jti == normalizedJti, cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            return;
        }

        _db.RevokedBusinessAccessTokens.Add(new RevokedBusinessAccessToken
        {
            Id = Guid.NewGuid(),
            Jti = normalizedJti,
            BusinessRegistrationId = businessRegistrationId,
            UserOrganizationId = userOrganizationId,
            ExpiresAtUtc = expiresAtUtc,
            RevokedAtUtc = DateTimeOffset.UtcNow,
        });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return false;
        }

        return await _db.RevokedBusinessAccessTokens
            .AsNoTracking()
            .AnyAsync(t => t.Jti == jti.Trim(), cancellationToken)
            .ConfigureAwait(false);
    }
}
