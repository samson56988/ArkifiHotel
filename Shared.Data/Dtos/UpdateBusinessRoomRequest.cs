namespace Shared.Data.Dtos;

public sealed class UpdateBusinessRoomRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    public decimal BasePricePerNight { get; set; }

    public IReadOnlyList<Guid>? AmenityIds { get; set; }
}
