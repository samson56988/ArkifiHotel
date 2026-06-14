namespace Shared.Data.Dtos;

public sealed class RequestPasswordResetData
{
    public string ChallengeId { get; set; } = string.Empty;

    public DateTimeOffset ChallengeExpiresAtUtc { get; set; }
}
