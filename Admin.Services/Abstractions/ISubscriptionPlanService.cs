using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface ISubscriptionPlanService
{
    Task<IReadOnlyList<SubscriptionPlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default);
}
