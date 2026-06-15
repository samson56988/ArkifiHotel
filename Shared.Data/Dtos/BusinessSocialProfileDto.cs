namespace Shared.Data.Dtos;

public sealed class BusinessSocialProfileDto
{
    public string? FacebookUrl { get; set; }

    public string? InstagramUrl { get; set; }

    public string? TikTokUrl { get; set; }

    public string? XUrl { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }
}

public sealed class UpdateBusinessSocialProfileRequest
{
    public string? FacebookUrl { get; set; }

    public string? InstagramUrl { get; set; }

    public string? TikTokUrl { get; set; }

    public string? XUrl { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }
}
