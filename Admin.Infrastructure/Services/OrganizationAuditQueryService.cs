using Admin.Data;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class OrganizationAuditQueryService : IOrganizationAuditQueryService
{
    private readonly AdminDbContext _db;

    public OrganizationAuditQueryService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<OrganizationAuditLogDto>> ListAsync(
        Guid businessId,
        ListOrganizationAuditQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize switch
        {
            < 1 => 50,
            > 100 => 100,
            _ => query.PageSize,
        };

        var logs = _db.OrganizationAuditLogs
            .AsNoTracking()
            .Where(l => l.BusinessRegistrationId == businessId);

        if (query.LocationId.HasValue)
        {
            logs = logs.Where(l => l.LocationId == query.LocationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            var entityType = query.EntityType.Trim().ToLowerInvariant();
            logs = logs.Where(l => l.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var action = query.Action.Trim().ToLowerInvariant();
            logs = logs.Where(l => l.Action == action);
        }

        if (query.UserOrganizationId.HasValue)
        {
            logs = logs.Where(l => l.UserOrganizationId == query.UserOrganizationId.Value);
        }

        if (query.FromUtc.HasValue)
        {
            logs = logs.Where(l => l.CreatedAt >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            logs = logs.Where(l => l.CreatedAt <= query.ToUtc.Value);
        }

        var totalCount = await logs.CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var rows = await logs
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResultDto<OrganizationAuditLogDto>
        {
            Items = rows.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }

    private static OrganizationAuditLogDto Map(Admin.Data.Entities.OrganizationAuditLog log) =>
        new()
        {
            Id = log.Id,
            UserOrganizationId = log.UserOrganizationId,
            UserDisplayName = log.UserDisplayName,
            UserEmail = log.UserEmail,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            LocationId = log.LocationId,
            LocationName = log.LocationName,
            Summary = log.Summary,
            DetailsJson = log.DetailsJson,
            CreatedAt = log.CreatedAt,
        };
}
