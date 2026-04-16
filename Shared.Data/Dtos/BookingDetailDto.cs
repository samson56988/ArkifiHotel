namespace Shared.Data.Dtos;

public sealed class BookingDetailDto
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string? GuestPhone { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int Nights { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string ConfirmationCode { get; set; } = string.Empty;

    public string? InternalNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
