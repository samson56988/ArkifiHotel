using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Services;

public sealed class OrganizationAuditService : IOrganizationAuditService
{
    private readonly AdminDbContext _db;
    private readonly IOrganizationUserContext _actor;

    public OrganizationAuditService(AdminDbContext db, IOrganizationUserContext actor)
    {
        _db = db;
        _actor = actor;
    }

    public async Task LogAsync(
        Guid businessId,
        OrganizationAuditEntry entry,
        Guid? userOrganizationId = null,
        string? userDisplayName = null,
        string? userEmail = null,
        CancellationToken cancellationToken = default)
    {
        var log = new OrganizationAuditLog
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            UserOrganizationId = userOrganizationId,
            UserDisplayName = Truncate(userDisplayName, 200),
            UserEmail = Truncate(userEmail, 320),
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            LocationId = entry.LocationId,
            LocationName = Truncate(entry.LocationName, 200),
            Summary = Truncate(entry.Summary, 500),
            DetailsJson = entry.DetailsJson,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.OrganizationAuditLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task LogForCurrentUserAsync(
        Guid businessId,
        OrganizationAuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        string? displayName = null;
        string? email = null;

        if (_actor.UserOrganizationId is not null)
        {
            return LogWithUserLookupAsync(businessId, entry, _actor.UserOrganizationId.Value, cancellationToken);
        }

        return LogAsync(businessId, entry, null, displayName, email, cancellationToken);
    }

    private async Task LogWithUserLookupAsync(
        Guid businessId,
        OrganizationAuditEntry entry,
        Guid userOrganizationId,
        CancellationToken cancellationToken)
    {
        var user = await _db.UserOrganizations
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userOrganizationId, cancellationToken)
            .ConfigureAwait(false);

        var displayName = user is null ? null : $"{user.FirstName} {user.LastName}".Trim();
        var email = user?.Email;

        await LogAsync(
            businessId,
            entry,
            userOrganizationId,
            displayName,
            email,
            cancellationToken).ConfigureAwait(false);
    }

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? value : value.Length <= max ? value : value[..max];
}
