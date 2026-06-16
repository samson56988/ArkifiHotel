namespace Admin.Data.Entities;

/// <summary>Guest-facing social links and contact details for a business storefront.</summary>
public class BusinessSocialProfile
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public string? FacebookUrl { get; set; }

    public string? FacebookHandle { get; set; }

    public string? FacebookFollowers { get; set; }

    public string? InstagramUrl { get; set; }

    public string? InstagramHandle { get; set; }

    public string? InstagramFollowers { get; set; }

    public string? TikTokUrl { get; set; }

    public string? TikTokHandle { get; set; }

    public string? TikTokFollowers { get; set; }

    public string? XUrl { get; set; }

    public string? XHandle { get; set; }

    public string? XFollowers { get; set; }

    /// <summary>Public contact email shown to guests (may differ from account login email).</summary>
    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
