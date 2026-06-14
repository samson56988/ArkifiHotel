namespace Shared.Data.Dtos;

public sealed class BookingPaymentSummaryDto
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public string BookingGuestName { get; set; } = string.Empty;

    public string BookingConfirmationCode { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string Status { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Gateway { get; set; } = string.Empty;

    public string? ExternalReference { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
