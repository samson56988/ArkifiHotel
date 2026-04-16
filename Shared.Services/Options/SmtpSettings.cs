namespace Shared.Services.Options;

public sealed class SmtpSettings
{
    public const string SectionName = "SmtpSettings";

    public string Server { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string SenderName { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool Ssl { get; set; } = true;

    public bool DefaultCredentials { get; set; }
}
