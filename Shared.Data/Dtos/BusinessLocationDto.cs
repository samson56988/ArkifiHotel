namespace Shared.Data.Dtos;

public sealed class BusinessLocationDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
