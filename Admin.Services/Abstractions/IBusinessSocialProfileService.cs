using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessSocialProfileService
{
    Task<BusinessSocialProfileDto> GetAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<(BusinessSocialProfileDto? Data, string? ErrorMessage)> UpdateAsync(
        Guid businessId,
        UpdateBusinessSocialProfileRequest request,
        CancellationToken cancellationToken = default);
}
