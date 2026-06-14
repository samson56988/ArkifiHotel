namespace Admin.Data.Entities;

/// <summary>OTP challenge for resetting a business account password.</summary>
public class BusinessPasswordResetChallenge
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
