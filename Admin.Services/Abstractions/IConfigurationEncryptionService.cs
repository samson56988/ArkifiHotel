namespace Admin.Services.Abstractions;

/// <summary>Encrypts and decrypts sensitive configuration strings using app settings key material.</summary>
public interface IConfigurationEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
