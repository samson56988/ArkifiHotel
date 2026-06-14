namespace Shared.Data.Dtos;

public sealed class UpdateBusinessLocationRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
}
