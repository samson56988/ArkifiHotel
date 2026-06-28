namespace Shared.Data.Dtos;

public sealed class BusinessRoomSummaryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Tagline { get; set; }

    public int MaxOccupancy { get; set; }

    public int? BedroomCount { get; set; }

    public int? BathroomCount { get; set; }

    public bool IsGuestFavorite { get; set; }

    public decimal BasePricePerNight { get; set; }

    public decimal? BasePricePerWeek { get; set; }

    public int Quantity { get; set; }

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public int AmenityCount { get; set; }

    public bool IsArchived { get; set; }
}
