namespace Shared.Data.Dtos;

public sealed class PropertyFacilitySummaryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PrimaryImageUrl { get; set; }

    public int ImageCount { get; set; }

    public bool IsArchived { get; set; }
}
