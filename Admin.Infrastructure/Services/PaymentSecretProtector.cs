using System.Text;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace Admin.Infrastructure.Services;

public sealed class PaymentSecretProtector : IPaymentSecretProtector
{
    private const string Purpose = "ArkifiHotel.V1.BusinessPaymentGatewaySecret";
    private readonly IDataProtector _protector;

    public PaymentSecretProtector(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string Protect(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(_protector.Protect(bytes));
    }

    public string? Unprotect(string? storedBase64)
    {
        if (string.IsNullOrWhiteSpace(storedBase64))
        {
            return null;
        }

        var bytes = Convert.FromBase64String(storedBase64);
        var plain = _protector.Unprotect(bytes);
        return Encoding.UTF8.GetString(plain);
    }
}
