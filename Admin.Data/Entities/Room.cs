namespace Admin.Data.Entities;

public class Room
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid? LocationId { get; set; }

    public BusinessLocation? Location { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    public decimal BasePricePerNight { get; set; }

    /// <summary>How many physical rooms of this type exist (e.g. 4 Executive rooms).</summary>
    public int Quantity { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>When true, the room is hidden from the default inventory list until restored or permanently deleted.</summary>
    public bool IsArchived { get; set; }

    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
