using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public enum PaymentConfigurationUpdateError
{
    NotFound,
    InvalidRequest,
}

public interface IBusinessPaymentConfigurationService
{
    Task<PaymentConfigurationDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<(PaymentConfigurationDto? Data, PaymentConfigurationUpdateError? Error)> UpdateAsync(
        Guid businessId,
        UpdatePaymentConfigurationRequest request,
        CancellationToken cancellationToken = default);
}
