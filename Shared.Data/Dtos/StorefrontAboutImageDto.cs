namespace Shared.Data.Dtos;

public sealed class StorefrontAboutImageDto
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }
}
