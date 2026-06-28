namespace Admin.Data.Entities;

/// <summary>JWTs revoked on business sign-out; kept until natural expiry.</summary>
public class RevokedBusinessAccessToken
{
    public Guid Id { get; set; }

    public string Jti { get; set; } = null!;

    public Guid BusinessRegistrationId { get; set; }

    public Guid? UserOrganizationId { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset RevokedAtUtc { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;
}
