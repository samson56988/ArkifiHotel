namespace Shared.Data.Dtos;

public sealed class CreatePropertyFacilityRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? LocationId { get; set; }
}
