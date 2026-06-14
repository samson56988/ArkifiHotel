using Admin.Data.Enums;

namespace Admin.Data.Entities;

/// <summary>Guest reservation for a room at a registered property.</summary>
public class Booking
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid RoomId { get; set; }

    public Room Room { get; set; } = null!;

    public Guid? LocationId { get; set; }

    public BusinessLocation? Location { get; set; }

    public string GuestName { get; set; } = null!;

    public string GuestEmail { get; set; } = null!;

    public string? GuestPhone { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public BookingStatus Status { get; set; }

    /// <summary>Guest-facing code for lookup without login (unique per business).</summary>
    public string ConfirmationCode { get; set; } = null!;

    public string? InternalNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
