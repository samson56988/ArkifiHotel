using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessPropertyFacilityService
{
    Task<IReadOnlyList<PropertyFacilitySummaryDto>> ListAsync(
        Guid businessId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    Task<PropertyFacilityDetailDto?> GetAsync(Guid businessId, Guid facilityId, CancellationToken cancellationToken = default);

    Task<PropertyFacilityDetailDto?> CreateAsync(
        Guid businessId,
        CreatePropertyFacilityRequest request,
        CancellationToken cancellationToken = default);

    Task<PropertyFacilityDetailDto?> UpdateAsync(
        Guid businessId,
        Guid facilityId,
        UpdatePropertyFacilityRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid businessId, Guid facilityId, CancellationToken cancellationToken = default);

    Task<bool> SetArchivedAsync(
        Guid businessId,
        Guid facilityId,
        bool archived,
        CancellationToken cancellationToken = default);

    Task<FacilityImageDto?> AddImageAsync(
        Guid businessId,
        Guid facilityId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(
        Guid businessId,
        Guid facilityId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}
