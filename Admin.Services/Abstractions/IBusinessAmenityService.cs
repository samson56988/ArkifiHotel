using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessAmenityService
{
    Task<IReadOnlyList<AmenityDto>> ListForBusinessAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<AmenityDto?> GetAsync(Guid businessId, Guid amenityId, CancellationToken cancellationToken = default);

    Task<AmenityDto?> CreateAsync(
        Guid businessId,
        CreateCustomAmenityRequest request,
        CancellationToken cancellationToken = default);

    Task<AmenityDto?> UpdateAsync(
        Guid businessId,
        Guid amenityId,
        UpdateAmenityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deleted, not found, or blocked because rooms still reference the amenity.</summary>
    Task<AmenityDeleteOutcome> DeleteAsync(
        Guid businessId,
        Guid amenityId,
        CancellationToken cancellationToken = default);
}

public enum AmenityDeleteOutcome
{
    Deleted,
    NotFound,
    InUseByRooms,
}
