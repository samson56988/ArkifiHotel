namespace Admin.Services.Abstractions;

public interface ICustomerConfirmationEmailService
{
    Task SendBookingConfirmationAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task SendRestaurantOrderConfirmationAsync(Guid orderId, CancellationToken cancellationToken = default);
}
