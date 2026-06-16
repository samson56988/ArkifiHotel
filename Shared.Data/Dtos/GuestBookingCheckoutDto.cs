namespace Shared.Data.Dtos;

public sealed class GuestBookingCheckoutDto
{
    public Guid BookingId { get; set; }

    public string PaymentReference { get; set; } = string.Empty;

    public string PaymentUrl { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";
}
