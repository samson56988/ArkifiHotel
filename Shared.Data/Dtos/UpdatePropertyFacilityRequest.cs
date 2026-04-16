namespace Shared.Data.Dtos;

public sealed class UpdatePropertyFacilityRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
