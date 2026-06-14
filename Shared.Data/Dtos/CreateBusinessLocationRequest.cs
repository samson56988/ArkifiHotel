namespace Shared.Data.Dtos;

public sealed class CreateBusinessLocationRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
}
