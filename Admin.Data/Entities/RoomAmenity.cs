namespace Admin.Data.Entities;

public class RoomAmenity
{
    public Guid RoomId { get; set; }

    public Room Room { get; set; } = null!;

    public Guid AmenityId { get; set; }

    public Amenity Amenity { get; set; } = null!;
}
