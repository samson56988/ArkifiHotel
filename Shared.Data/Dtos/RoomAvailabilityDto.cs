namespace Shared.Data.Dtos;

public sealed class RoomAvailabilityDto
{
    public Guid RoomId { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public int TotalQuantity { get; set; }

    /// <summary>Peak concurrent bookings during the requested stay dates.</summary>
    public int PeakBooked { get; set; }

    public int AvailableUnits { get; set; }

    public bool IsAvailable { get; set; }

    public decimal BasePricePerNight { get; set; }

    public decimal? BasePricePerWeek { get; set; }

    public int MaxOccupancy { get; set; }

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }
}
