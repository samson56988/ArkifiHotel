namespace Admin.Data.Entities;

/// <summary>Physical site or branch where rooms and facilities can be placed.</summary>
public class BusinessLocation
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public ICollection<PropertyFacility> PropertyFacilities { get; set; } = new List<PropertyFacility>();
}
