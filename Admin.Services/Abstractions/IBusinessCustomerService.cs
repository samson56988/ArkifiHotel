using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessCustomerService
{
    Task<IReadOnlyList<CustomerSummaryDto>> ListAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> GetAsync(Guid businessId, Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> CreateAsync(Guid businessId, CreateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> UpdateAsync(
        Guid businessId,
        Guid customerId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid businessId, Guid customerId, CancellationToken cancellationToken = default);
}
