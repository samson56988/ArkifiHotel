namespace Admin.Data.Entities;

public class EventHallImage
{
    public Guid Id { get; set; }

    public Guid EventHallId { get; set; }

    public EventHall EventHall { get; set; } = null!;

    public string RelativePath { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
