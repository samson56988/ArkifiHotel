using Shared.Data.Dtos;

namespace Admin.Services.Abstractions;

public interface IBusinessBookingPaymentService
{
    Task<IReadOnlyList<BookingPaymentSummaryDto>> ListAsync(Guid businessId, CancellationToken cancellationToken = default);

    Task<BookingPaymentSummaryDto?> CreateAsync(
        Guid businessId,
        CreateBookingPaymentRequest request,
        CancellationToken cancellationToken = default);
}
