using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IPlatformSubscriptionAdminService
{
    Task<IReadOnlyList<SubscriptionPlanDto>> ListPlansAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformSubscriptionPaymentDto>> ListPaymentsAsync(
        CancellationToken cancellationToken = default);
}
