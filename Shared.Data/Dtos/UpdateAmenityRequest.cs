namespace Shared.Data.Dtos;

public sealed class UpdateAmenityRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }
}
