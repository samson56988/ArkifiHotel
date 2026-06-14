using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public enum BusinessProfileUpdateError
{
    None,
    NotFound,
    Validation,
    DuplicateSlug,
}

public interface IBusinessProfileService
{
    Task<BusinessProfileDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<(BusinessProfileDto? Data, BusinessProfileUpdateError Error, string? Message)> UpdateAsync(
        Guid businessId,
        UpdateBusinessProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<(BusinessProfileDto? Data, BusinessProfileUpdateError Error, string? Message)> UpdateLogoPathAsync(
        Guid businessId,
        string? relativeLogoPath,
        CancellationToken cancellationToken = default);

    Task<SlugAvailabilityDto> CheckSlugAvailabilityAsync(
        string slug,
        Guid? excludeBusinessId = null,
        CancellationToken cancellationToken = default);
}
