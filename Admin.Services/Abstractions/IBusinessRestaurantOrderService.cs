using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessRestaurantOrderService
{
    Task<RestaurantOrderListResultDto> ListAsync(
        Guid businessId,
        int page,
        int pageSize,
        string? status,
        CancellationToken cancellationToken = default);

    Task<RestaurantOrderDetailDto?> GetAsync(
        Guid businessId,
        Guid orderId,
        CancellationToken cancellationToken = default);
}
