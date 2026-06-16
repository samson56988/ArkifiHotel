namespace Shared.Data.Dtos;

public sealed class GuestPaymentVerifyResultDto
{
    public bool PaymentSuccessful { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Message { get; set; }

    public GuestBookingLookupDto? Booking { get; set; }
}
