using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IStorefrontBannerImageService
{
    const int MaxImages = 3;

    Task<IReadOnlyList<StorefrontBannerImageDto>> GetAsync(
        Guid businessId,
        Guid? locationId = null,
        CancellationToken cancellationToken = default);

    Task<StorefrontBannerImageDto?> AddImageAsync(
        Guid businessId,
        Guid locationId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid businessId, Guid locationId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid businessId, Guid imageId, CancellationToken cancellationToken = default);
}
