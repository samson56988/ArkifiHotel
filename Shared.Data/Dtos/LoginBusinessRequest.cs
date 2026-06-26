namespace Shared.Data.Dtos;

public sealed class LoginBusinessRequest
{
    /// <summary>Email (super-admin) or business-slug/username (staff).</summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>Legacy alias for <see cref="Login"/>.</summary>
    public string Email
    {
        get => Login;
        set => Login = value;
    }

    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
