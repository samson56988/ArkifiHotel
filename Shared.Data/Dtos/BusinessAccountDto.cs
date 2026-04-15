namespace Shared.Data.Dtos;

/// <summary>Business account summary returned from login (no secrets).</summary>
public sealed class BusinessAccountDto
{
    public Guid Id { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    /// <summary>"Active" or "Inactive".</summary>
    public string Status { get; set; } = string.Empty;
}
