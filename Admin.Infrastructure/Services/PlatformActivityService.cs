using Admin.Data;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class PlatformActivityService : IPlatformActivityService
{
    private readonly AdminDbContext _db;

    public PlatformActivityService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<PlatformActivityLogDto>> ListAsync(
        ListPlatformActivityQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 50 : query.PageSize;

        var logs = _db.OrganizationAuditLogs.AsNoTracking().AsQueryable();

        if (query.BusinessId is Guid businessId)
        {
            logs = logs.Where(l => l.BusinessRegistrationId == businessId);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            var entityType = query.EntityType.Trim();
            logs = logs.Where(l => l.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var action = query.Action.Trim();
            logs = logs.Where(l => l.Action == action);
        }

        if (query.FromUtc is DateTimeOffset fromUtc)
        {
            logs = logs.Where(l => l.CreatedAt >= fromUtc);
        }

        if (query.ToUtc is DateTimeOffset toUtc)
        {
            logs = logs.Where(l => l.CreatedAt <= toUtc);
        }

        var totalCount = await logs.CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await logs
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new PlatformActivityLogDto
            {
                Id = l.Id,
                BusinessId = l.BusinessRegistrationId,
                BusinessName = l.BusinessRegistration.BusinessName,
                EntityType = l.EntityType,
                Action = l.Action,
                EntityId = l.EntityId.HasValue ? l.EntityId.Value.ToString() : null,
                Summary = l.Summary,
                ActorName = l.UserDisplayName ?? l.UserEmail,
                CreatedAt = l.CreatedAt,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResultDto<PlatformActivityLogDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }
}
