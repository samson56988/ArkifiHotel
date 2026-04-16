namespace Shared.Data.Dtos;

public sealed class BusinessRoomSummaryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int MaxOccupancy { get; set; }

    public decimal BasePricePerNight { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public int AmenityCount { get; set; }
}
