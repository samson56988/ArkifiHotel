namespace Shared.Data.Dtos;

/// <summary>Limited fields returned for anonymous booking lookup by confirmation code.</summary>
public sealed class GuestBookingLookupDto
{
    public string PropertyName { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    public string GuestName { get; set; } = string.Empty;

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int Nights { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string ConfirmationCode { get; set; } = string.Empty;
}
