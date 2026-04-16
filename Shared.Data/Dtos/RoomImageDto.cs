namespace Shared.Data.Dtos;

public sealed class RoomImageDto
{
    public Guid Id { get; set; }

    /// <summary>URL path under the API host (starts with /uploads/...).</summary>
    public string Url { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public int SortOrder { get; set; }
}
