using Admin.Data.Enums;

namespace Admin.Data.Entities;

public class EventHallRequest
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid LocationId { get; set; }

    public BusinessLocation Location { get; set; } = null!;

    public Guid EventHallId { get; set; }

    public EventHall EventHall { get; set; } = null!;

    public string GuestName { get; set; } = null!;

    public string GuestEmail { get; set; } = null!;

    public string GuestPhone { get; set; } = null!;

    public DateOnly EventDate { get; set; }

    public DateOnly? EventEndDate { get; set; }

    /// <summary>Why the guest needs the hall (wedding, conference, etc.).</summary>
    public string EventPurpose { get; set; } = null!;

    public string? Notes { get; set; }

    public EventHallRequestStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
