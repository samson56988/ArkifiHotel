using Admin.Data;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessPaymentConfigurationService : IBusinessPaymentConfigurationService
{
    private const int MinSecretLength = 8;
    private const int MaxSecretLength = 512;

    private readonly AdminDbContext _db;
    private readonly IPaymentSecretProtector _crypto;

    public BusinessPaymentConfigurationService(AdminDbContext db, IPaymentSecretProtector crypto)
    {
        _db = db;
        _crypto = crypto;
    }

    public async Task<PaymentConfigurationDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var row = await _db.BusinessRegistrations
            .AsNoTracking()
            .Select(b => new { b.Id, b.PaymentProvider, b.PaymentSecretProtected })
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
        {
            return null;
        }

        return new PaymentConfigurationDto
        {
            Provider = row.PaymentProvider.ToString(),
            HasSecretKey = !string.IsNullOrEmpty(row.PaymentSecretProtected),
        };
    }

    public async Task<(PaymentConfigurationDto? Data, PaymentConfigurationUpdateError? Error)> UpdateAsync(
        Guid businessId,
        UpdatePaymentConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return (null, PaymentConfigurationUpdateError.NotFound);
        }

        if (!TryParseProvider(request.Provider, out var newProvider))
        {
            return (null, PaymentConfigurationUpdateError.InvalidRequest);
        }

        var secret = request.SecretKey?.Trim();
        var hasIncomingSecret = !string.IsNullOrEmpty(secret);
        if (hasIncomingSecret && (secret!.Length < MinSecretLength || secret.Length > MaxSecretLength))
        {
            return (null, PaymentConfigurationUpdateError.InvalidRequest);
        }

        if (newProvider == PaymentGatewayProvider.None)
        {
            entity.PaymentProvider = PaymentGatewayProvider.None;
            entity.PaymentSecretProtected = null;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var cleared = await MapAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            return (cleared, null);
        }

        var sameProvider = entity.PaymentProvider == newProvider;
        var hadSecret = !string.IsNullOrEmpty(entity.PaymentSecretProtected);

        if (!hasIncomingSecret)
        {
            if (sameProvider && hadSecret)
            {
                entity.PaymentProvider = newProvider;
                entity.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                var kept = await MapAsync(entity.Id, cancellationToken).ConfigureAwait(false);
                return (kept, null);
            }

            return (null, PaymentConfigurationUpdateError.InvalidRequest);
        }

        entity.PaymentProvider = newProvider;
        entity.PaymentSecretProtected = _crypto.Protect(secret!);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var saved = await MapAsync(entity.Id, cancellationToken).ConfigureAwait(false);
        return (saved, null);
    }

    private async Task<PaymentConfigurationDto> MapAsync(Guid businessId, CancellationToken cancellationToken)
    {
        var row = await _db.BusinessRegistrations
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(b => new { b.PaymentProvider, b.PaymentSecretProtected })
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PaymentConfigurationDto
        {
            Provider = row.PaymentProvider.ToString(),
            HasSecretKey = !string.IsNullOrEmpty(row.PaymentSecretProtected),
        };
    }

    private static bool TryParseProvider(string? value, out PaymentGatewayProvider provider)
    {
        provider = PaymentGatewayProvider.None;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out provider)
               && Enum.IsDefined(typeof(PaymentGatewayProvider), provider);
    }
}
