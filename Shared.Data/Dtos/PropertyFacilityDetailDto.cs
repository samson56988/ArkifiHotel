namespace Shared.Data.Dtos;

public sealed class PropertyFacilityDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? LocationId { get; set; }

    public string? LocationName { get; set; }

    public IReadOnlyList<FacilityImageDto> Images { get; set; } = Array.Empty<FacilityImageDto>();

    public bool IsArchived { get; set; }
}
