namespace Admin.Data.Entities;

/// <summary>
/// Login 2FA OTP challenge for business sign-in.
/// </summary>
public class BusinessLoginOtpChallenge
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public string OtpCodeHash { get; set; } = null!;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;
}
