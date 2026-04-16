using Admin.Data.Enums;

namespace Admin.Data.Entities;

/// <summary>Payment record linked to a booking (gateway or manually logged).</summary>
public class BookingPayment
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public BookingPaymentStatus Status { get; set; }

    /// <summary>Paystack, Flutterwave, or None for manual/offline entries.</summary>
    public PaymentGatewayProvider Gateway { get; set; }

    /// <summary>Gateway transaction or reference id.</summary>
    public string? ExternalReference { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
