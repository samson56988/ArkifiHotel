using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IPlatformBusinessService
{
    Task<PlatformDashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformBusinessSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<PlatformBusinessDetailDto?> GetByIdAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<(PlatformBusinessDetailDto? Data, string? Error)> UpdateAsync(
        Guid businessId,
        UpdatePlatformBusinessRequest request,
        CancellationToken cancellationToken = default);
}
