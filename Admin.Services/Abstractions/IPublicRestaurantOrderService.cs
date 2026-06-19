using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public enum PublicRestaurantOrderError
{
    NotFound,
    InvalidRequest,
    PaymentNotConfigured,
    PaymentInitFailed,
    ItemUnavailable,
}

public interface IPublicRestaurantOrderService
{
    Task<(GuestRestaurantOrderCheckoutDto? Data, PublicRestaurantOrderError? Error, string? Message)> CreateCheckoutAsync(
        string slug,
        GuestCreateRestaurantOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<GuestRestaurantOrderVerifyResultDto?> VerifyPaymentAsync(
        string slug,
        string paymentReference,
        CancellationToken cancellationToken = default);
}
