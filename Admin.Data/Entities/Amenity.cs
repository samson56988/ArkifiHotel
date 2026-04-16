namespace Admin.Data.Entities;

/// <summary>
/// Room amenity. When <see cref="BusinessRegistrationId"/> is null, the row is part of the global catalog.
/// When set, the amenity is owned by that business (custom).
/// </summary>
public class Amenity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public Guid? BusinessRegistrationId { get; set; }

    public BusinessRegistration? BusinessRegistration { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
