namespace Admin.Data.Entities;

/// <summary>ArkifiStay internal staff who manage businesses on the platform admin app.</summary>
public class PlatformStaff
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string HashedPassword { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
