using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessSubscriptionService
{
    Task<BusinessSubscriptionDto?> GetCurrentAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionPlanOptionDto>> GetPlanOptionsAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    Task<(InitSubscriptionPaymentResultDto? Data, string? Error)> InitializePaymentAsync(
        Guid businessId,
        string planCode,
        CancellationToken cancellationToken = default);

    Task<(BusinessSubscriptionDto? Data, string? Error)> VerifyPaymentAsync(
        Guid businessId,
        string paymentReference,
        CancellationToken cancellationToken = default);

    Task<(BusinessSubscriptionDto? Data, string? Error)> ChangePlanWithoutPaymentAsync(
        Guid businessId,
        string planCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BusinessSubscriptionPaymentHistoryDto>> GetPaymentHistoryAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);
}
