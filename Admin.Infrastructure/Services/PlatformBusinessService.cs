using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class PlatformBusinessService : IPlatformBusinessService
{
    private readonly AdminDbContext _db;

    public PlatformBusinessService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<PlatformDashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var businesses = await _db.BusinessRegistrations
            .AsNoTracking()
            .Select(b => new { b.Status, b.BusinessType, b.SubscriptionPlanId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var proPlanIds = await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.Tier == SubscriptionPlanTier.Pro)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var weekAgo = DateTimeOffset.UtcNow.AddDays(-7);
        var recentActivity = await _db.OrganizationAuditLogs
            .AsNoTracking()
            .CountAsync(l => l.CreatedAt >= weekAgo, cancellationToken)
            .ConfigureAwait(false);

        return new PlatformDashboardStatsDto
        {
            TotalBusinesses = businesses.Count,
            ActiveBusinesses = businesses.Count(b => b.Status == BusinessRegistrationStatus.Active),
            HotelCount = businesses.Count(b => b.BusinessType == BusinessType.Hotel),
            ShortletCount = businesses.Count(b => b.BusinessType == BusinessType.Shortlet),
            ProSubscriptions = businesses.Count(b => proPlanIds.Contains(b.SubscriptionPlanId)),
            RecentActivityCount = recentActivity,
        };
    }

    public async Task<IReadOnlyList<PlatformBusinessSummaryDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.BusinessRegistrations
            .AsNoTracking()
            .Include(b => b.SubscriptionPlan)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(MapSummary).ToList();
    }

    public async Task<PlatformBusinessDetailDto?> GetByIdAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessRegistrations
            .AsNoTracking()
            .Include(b => b.SubscriptionPlan)
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return null;
        }

        var locationCount = await _db.BusinessLocations
            .AsNoTracking()
            .CountAsync(l => l.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        var roomCount = await _db.Rooms
            .AsNoTracking()
            .CountAsync(r => r.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        var bookingCount = await _db.Bookings
            .AsNoTracking()
            .CountAsync(b => b.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        var staffCount = await _db.UserOrganizations
            .AsNoTracking()
            .CountAsync(u => u.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        var summary = MapSummary(entity);
        return new PlatformBusinessDetailDto
        {
            Id = summary.Id,
            BusinessName = summary.BusinessName,
            Slug = summary.Slug,
            BusinessType = summary.BusinessType,
            Status = summary.Status,
            ContactEmail = summary.ContactEmail,
            IsEmailVerified = summary.IsEmailVerified,
            PlanName = summary.PlanName,
            PlanTier = summary.PlanTier,
            SubscriptionExpiresAt = summary.SubscriptionExpiresAt,
            CreatedAt = summary.CreatedAt,
            AdminNotes = summary.AdminNotes,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            PhoneNumber = entity.PhoneNumber,
            LocationCount = locationCount,
            RoomCount = roomCount,
            BookingCount = bookingCount,
            StaffCount = staffCount,
        };
    }

    public async Task<(PlatformBusinessDetailDto? Data, string? Error)> UpdateAsync(
        Guid businessId,
        UpdatePlatformBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return (null, "Business not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<BusinessRegistrationStatus>(request.Status, ignoreCase: true, out var status))
            {
                return (null, "Invalid status. Use Active or Inactive.");
            }

            entity.Status = status;
        }

        if (request.AdminNotes is not null)
        {
            entity.AdminNotes = string.IsNullOrWhiteSpace(request.AdminNotes) ? null : request.AdminNotes.Trim();
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var detail = await GetByIdAsync(businessId, cancellationToken).ConfigureAwait(false);
        return (detail, null);
    }

    private static PlatformBusinessSummaryDto MapSummary(BusinessRegistration entity) =>
        new()
        {
            Id = entity.Id,
            BusinessName = entity.BusinessName,
            Slug = entity.Slug,
            BusinessType = entity.BusinessType == BusinessType.Shortlet ? "Shortlet" : "Hotel",
            Status = entity.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive",
            ContactEmail = entity.ContactEmail,
            IsEmailVerified = entity.IsEmailVerified,
            PlanName = entity.SubscriptionPlan?.Name ?? string.Empty,
            PlanTier = entity.SubscriptionPlan?.Tier == SubscriptionPlanTier.Pro ? "Pro" : "Free",
            SubscriptionExpiresAt = entity.SubscriptionExpiresAt,
            CreatedAt = entity.CreatedAt,
            AdminNotes = entity.AdminNotes,
        };
}
