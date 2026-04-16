namespace Admin.Data.Entities;

/// <summary>
/// Hotel-wide shared spaces (e.g. swimming pool, gym, lounge) — not tied to a single guest room.
/// </summary>
public class PropertyFacility
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<PropertyFacilityImage> Images { get; set; } = new List<PropertyFacilityImage>();
}
