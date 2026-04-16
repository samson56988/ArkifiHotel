namespace Shared.Data.Dtos;

public sealed class AmenityDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    /// <summary>True when created by the signed-in business (not platform catalog).</summary>
    public bool IsCustom { get; set; }
}
