namespace Shared.Data.Dtos;

public sealed class BusinessRoomDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    public decimal BasePricePerNight { get; set; }

    public IReadOnlyList<RoomImageDto> Images { get; set; } = Array.Empty<RoomImageDto>();

    public IReadOnlyList<AmenityDto> Amenities { get; set; } = Array.Empty<AmenityDto>();

    public bool IsArchived { get; set; }
}
