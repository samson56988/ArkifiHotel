namespace Shared.Data.Dtos;

/// <summary>Successful business login payload (JWT + account).</summary>
public sealed class LoginBusinessData
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public BusinessAccountDto Account { get; set; } = null!;
}
