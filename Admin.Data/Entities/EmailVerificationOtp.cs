namespace Admin.Data.Entities;

/// <summary>
/// One-time code issued to verify a business contact email.
/// </summary>
public class EmailVerificationOtp
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    /// <summary>
    /// SHA256 hash of the OTP code. Never store OTP in plain text.
    /// </summary>
    public string OtpCodeHash { get; set; } = null!;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;
}
