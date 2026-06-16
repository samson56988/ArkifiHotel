namespace Shared.Data.Dtos;

public sealed class PublicStorefrontLocationDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
}
