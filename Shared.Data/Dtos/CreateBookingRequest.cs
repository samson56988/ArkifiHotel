namespace Shared.Data.Dtos;

public sealed class CreateBookingRequest
{
    public Guid RoomId { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public string? GuestPhone { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public string? InternalNotes { get; set; }
}
