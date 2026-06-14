namespace Shared.Data.Dtos;

public sealed class UpdateBusinessProfileRequest
{
    public string BusinessName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;
}
