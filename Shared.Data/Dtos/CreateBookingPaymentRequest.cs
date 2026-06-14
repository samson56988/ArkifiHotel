namespace Shared.Data.Dtos;

public sealed class CreateBookingPaymentRequest
{
    public Guid BookingId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    /// <summary>Pending, Completed, Failed, Refunded, or Cancelled.</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Cash, BankTransfer, or Gateway.</summary>
    public string Method { get; set; } = "Cash";

    /// <summary>When Method is Gateway: Paystack or Flutterwave. Otherwise None.</summary>
    public string Gateway { get; set; } = "None";

    public string? ExternalReference { get; set; }

    public string? Notes { get; set; }
}
