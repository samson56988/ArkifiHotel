using Admin.Data.Enums;

namespace Admin.Data.Entities;

public class RestaurantOrderPayment
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid RestaurantOrderId { get; set; }

    public RestaurantOrder RestaurantOrder { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public BookingPaymentStatus Status { get; set; }

    public BookingPaymentMethod Method { get; set; }

    public PaymentGatewayProvider Gateway { get; set; }

    public string? ExternalReference { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
