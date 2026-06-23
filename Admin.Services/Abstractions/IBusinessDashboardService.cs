using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessDashboardService
{
    Task<BusinessDashboardDto?> GetDashboardAsync(
        Guid businessId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default);
}
