using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessBookingService
{
    Task<PagedResultDto<BookingSummaryDto>> ListAsync(
        Guid businessId,
        ListBookingsQuery query,
        CancellationToken cancellationToken = default);

    Task<BookingDetailDto?> GetAsync(Guid businessId, Guid bookingId, CancellationToken cancellationToken = default);

    Task<BookingDetailDto?> CreateAsync(Guid businessId, CreateBookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingDetailDto?> UpdateStatusAsync(
        Guid businessId,
        Guid bookingId,
        string status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoomAvailabilityDto>> GetAvailabilityAsync(
        Guid businessId,
        DateOnly checkIn,
        DateOnly checkOut,
        Guid? roomId = null,
        Guid? locationId = null,
        CancellationToken cancellationToken = default);
}
