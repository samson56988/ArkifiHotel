namespace Shared.Data.Dtos;

public sealed class BookingSummaryDto
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int Nights { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string ConfirmationCode { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>True when checkout date is today or in the past (stay has ended).</summary>
    public bool IsStayClosed { get; set; }
}
