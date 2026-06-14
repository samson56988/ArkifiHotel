using System.Security.Cryptography;
using System.Text;
using Admin.Infrastructure.Options;
using Admin.Services.Abstractions;
using Microsoft.Extensions.Options;

namespace Admin.Infrastructure.Services;

public sealed class ConfigurationEncryptionService : IConfigurationEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public ConfigurationEncryptionService(IOptions<EncryptionSettings> options)
    {
        var keyMaterial = options.Value.Key?.Trim();
        if (string.IsNullOrEmpty(keyMaterial))
        {
            throw new InvalidOperationException(
                "EncryptionSettings:Key is required. Set it in appsettings for payment credential encryption.");
        }

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var payload = new byte[NonceSize + cipherBytes.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(cipherBytes, 0, payload, NonceSize, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, payload, NonceSize + cipherBytes.Length, TagSize);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        var payload = Convert.FromBase64String(cipherText);
        if (payload.Length <= NonceSize + TagSize)
        {
            throw new CryptographicException("Invalid encrypted payload.");
        }

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(payload.Length - TagSize, TagSize);
        var cipherBytes = payload.AsSpan(NonceSize, payload.Length - NonceSize - TagSize);
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
