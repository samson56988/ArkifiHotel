namespace Shared.Data.Dtos;

public sealed class CreateBookingRequest
{
    public Guid LocationId { get; set; }

    public Guid RoomId { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string GuestPhone { get; set; } = string.Empty;

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public string? InternalNotes { get; set; }

    /// <summary>Cash or BankTransfer only (reception desk).</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Reception must confirm payment was received before creating the booking.</summary>
    public bool PaymentConfirmed { get; set; }
}
