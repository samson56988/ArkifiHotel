namespace Shared.Data.Dtos;

public sealed class SlugAvailabilityDto
{
    public string Slug { get; set; } = string.Empty;

    public bool Available { get; set; }
}
