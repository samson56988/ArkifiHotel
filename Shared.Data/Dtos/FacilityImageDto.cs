namespace Shared.Data.Dtos;

public sealed class FacilityImageDto
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public int SortOrder { get; set; }
}
