namespace Shared.Data.Dtos;

public sealed class VerifyLoginOtpRequest
{
    public string Email { get; set; } = string.Empty;

    public string Otp { get; set; } = string.Empty;

    public string ChallengeId { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
