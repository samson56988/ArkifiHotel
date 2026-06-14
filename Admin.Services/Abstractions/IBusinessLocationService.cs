using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessLocationService
{
    Task<IReadOnlyList<BusinessLocationDto>> ListAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<BusinessLocationDto?> GetAsync(Guid businessId, Guid locationId, CancellationToken cancellationToken = default);

    Task<BusinessLocationDto?> CreateAsync(
        Guid businessId,
        CreateBusinessLocationRequest request,
        CancellationToken cancellationToken = default);

    Task<BusinessLocationDto?> UpdateAsync(
        Guid businessId,
        Guid locationId,
        UpdateBusinessLocationRequest request,
        CancellationToken cancellationToken = default);

    Task<LocationDeleteOutcome> DeleteAsync(
        Guid businessId,
        Guid locationId,
        CancellationToken cancellationToken = default);
}
