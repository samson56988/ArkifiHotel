using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessBookingService
{
    Task<IReadOnlyList<BookingSummaryDto>> ListAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<BookingDetailDto?> GetAsync(Guid businessId, Guid bookingId, CancellationToken cancellationToken = default);

    Task<BookingDetailDto?> CreateAsync(Guid businessId, CreateBookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingDetailDto?> UpdateStatusAsync(
        Guid businessId,
        Guid bookingId,
        string status,
        CancellationToken cancellationToken = default);
}
