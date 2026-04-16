namespace Admin.Services.Abstractions;

/// <summary>Encrypts and decrypts payment gateway secret keys using the host data-protection key ring.</summary>
public interface IPaymentSecretProtector
{
    string Protect(string plainText);

    string? Unprotect(string? storedBase64);
}
