namespace Shared.Data.Dtos;

public sealed class CreateBookingPaymentRequest
{
    public Guid BookingId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    /// <summary>Pending, Completed, Failed, Refunded, or Cancelled.</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>None, Paystack, or Flutterwave.</summary>
    public string Gateway { get; set; } = "None";

    public string? ExternalReference { get; set; }

    public string? Notes { get; set; }
}
