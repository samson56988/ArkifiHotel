using Admin.Data;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class PlatformSubscriptionAdminService : IPlatformSubscriptionAdminService
{
    private readonly AdminDbContext _db;

    public PlatformSubscriptionAdminService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> ListPlansAsync(
        CancellationToken cancellationToken = default)
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return plans.Select(SubscriptionPlanService.Map).ToList();
    }

    public async Task<IReadOnlyList<PlatformSubscriptionPaymentDto>> ListPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.BusinessSubscriptionPayments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PlatformSubscriptionPaymentDto
            {
                Id = p.Id,
                BusinessId = p.BusinessRegistrationId,
                BusinessName = p.BusinessRegistration.BusinessName,
                PlanName = p.TargetPlan.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = MapPaymentStatus(p.Status),
                PaymentReference = p.PaymentReference,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static string MapPaymentStatus(BusinessSubscriptionPaymentStatus status) =>
        status switch
        {
            BusinessSubscriptionPaymentStatus.Pending => "Pending",
            BusinessSubscriptionPaymentStatus.Completed => "Completed",
            BusinessSubscriptionPaymentStatus.Failed => "Failed",
            _ => status.ToString(),
        };
}
