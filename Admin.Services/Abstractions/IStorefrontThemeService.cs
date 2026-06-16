using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IStorefrontThemeService
{
    Task<StorefrontThemeDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<(StorefrontThemeDto? Theme, string? ErrorMessage)> UpdateAsync(
        Guid businessId,
        StorefrontThemeDto theme,
        CancellationToken cancellationToken = default);

    Task<PublicStorefrontDto?> GetPublicBySlugAsync(
        string slug,
        Guid? locationId = null,
        CancellationToken cancellationToken = default);
}
