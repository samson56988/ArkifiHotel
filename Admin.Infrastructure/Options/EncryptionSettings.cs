namespace Admin.Infrastructure.Options;

public sealed class EncryptionSettings
{
    public const string SectionName = "EncryptionSettings";

    /// <summary>Symmetric key used to encrypt payment gateway credential JSON at rest.</summary>
    public string Key { get; set; } = string.Empty;
}
