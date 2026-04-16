namespace Shared.Data.Dtos;

/// <summary>Successful business login payload (JWT + account).</summary>
public sealed class LoginBusinessData
{
    public string? AccessToken { get; set; }

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public BusinessAccountDto? Account { get; set; }

    public bool RequiresTwoFactor { get; set; }

    public string? ChallengeId { get; set; }

    public DateTimeOffset? ChallengeExpiresAtUtc { get; set; }
}
