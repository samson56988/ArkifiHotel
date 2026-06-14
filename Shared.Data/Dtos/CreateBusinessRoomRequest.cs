namespace Shared.Data.Dtos;

public sealed class CreateBusinessRoomRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    public decimal BasePricePerNight { get; set; }

    public int Quantity { get; set; }

    public Guid? LocationId { get; set; }

    public IReadOnlyList<Guid>? AmenityIds { get; set; }
}
