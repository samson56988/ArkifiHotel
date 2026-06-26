namespace Shared.Data.Dtos;

public sealed class BusinessRoomDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Tagline { get; set; }

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    public int? BedroomCount { get; set; }

    public int? BathroomCount { get; set; }

    public bool IsGuestFavorite { get; set; }

    public decimal BasePricePerNight { get; set; }

    public int Quantity { get; set; }

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }

    public IReadOnlyList<RoomImageDto> Images { get; set; } = Array.Empty<RoomImageDto>();

    public IReadOnlyList<AmenityDto> Amenities { get; set; } = Array.Empty<AmenityDto>();

    public bool IsArchived { get; set; }
}
