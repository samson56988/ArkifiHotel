namespace Admin.Data.Entities;

public class Room
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    public decimal BasePricePerNight { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
