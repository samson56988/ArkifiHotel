using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IPlatformActivityService
{
    Task<PagedResultDto<PlatformActivityLogDto>> ListAsync(
        ListPlatformActivityQuery query,
        CancellationToken cancellationToken = default);
}
