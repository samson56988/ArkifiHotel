namespace Shared.Data.Dtos;

public sealed class CreateCustomAmenityRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }
}
