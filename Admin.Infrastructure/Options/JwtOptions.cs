namespace Admin.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Symmetric key for signing HS256 tokens (use a long random secret in production).</summary>
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "ArkifiHub";

    public string Audience { get; set; } = "ArkifiHub.Business";

    /// <summary>Access token lifetime in minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 120;

    /// <summary>When true, <see cref="RememberMe"/> extends token lifetime (e.g. 30 days).</summary>
    public int RememberMeAccessTokenDays { get; set; } = 30;
}
