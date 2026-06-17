using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public enum PublicGuestBookingError
{
    NotFound,
    InvalidRequest,
    RoomUnavailable,
    PaymentNotConfigured,
    PaymentInitFailed,
}

public interface IPublicGuestBookingService
{
    Task<(GuestBookingCheckoutDto? Data, PublicGuestBookingError? Error, string? Message)> CreateCheckoutAsync(
        string slug,
        GuestCreateBookingRequest request,
        CancellationToken cancellationToken = default);

    Task<GuestPaymentVerifyResultDto?> VerifyPaymentAsync(
        string slug,
        string paymentReference,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<RoomAvailabilityDto>? Data, PublicGuestBookingError? Error, string? Message)> GetRoomAvailabilityAsync(
        string slug,
        Guid locationId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default);
}
