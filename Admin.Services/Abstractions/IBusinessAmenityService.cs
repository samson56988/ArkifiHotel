using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessAmenityService
{
    Task<IReadOnlyList<AmenityDto>> ListForBusinessAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<AmenityDto?> CreateCustomAsync(
        Guid businessId,
        CreateCustomAmenityRequest request,
        CancellationToken cancellationToken = default);
}
