namespace Admin.Data.Entities;

/// <summary>Rentable event space at a hotel branch (requests only — no online payment).</summary>
public class EventHall
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid LocationId { get; set; }

    public BusinessLocation Location { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal RentalPrice { get; set; }

    public int? MaxCapacity { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<EventHallImage> Images { get; set; } = new List<EventHallImage>();
}
