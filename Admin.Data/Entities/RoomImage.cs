namespace Admin.Data.Entities;

public class RoomImage
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }

    public Room Room { get; set; } = null!;

    /// <summary>Path relative to wwwroot, e.g. uploads/{businessId}/{roomId}/file.jpg</summary>
    public string RelativePath { get; set; } = null!;

    public string? OriginalFileName { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
