using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IStorefrontAboutImageService
{
    Task<StorefrontAboutImageDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<StorefrontAboutImageDto?> UpsertAsync(
        Guid businessId,
        string relativePathUnderWwwroot,
        string? originalFileName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid businessId, CancellationToken cancellationToken = default);
}
