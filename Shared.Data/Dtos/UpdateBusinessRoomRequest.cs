namespace Shared.Data.Dtos;

public sealed class UpdateBusinessRoomRequest
{
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

    public IReadOnlyList<Guid>? AmenityIds { get; set; }
}
