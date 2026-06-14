using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessPaymentConfigurationService : IBusinessPaymentConfigurationService
{
    private const int MinFieldLength = 4;
    private const int MaxFieldLength = 512;

    private readonly AdminDbContext _db;
    private readonly IConfigurationEncryptionService _crypto;

    public BusinessPaymentConfigurationService(AdminDbContext db, IConfigurationEncryptionService crypto)
    {
        _db = db;
        _crypto = crypto;
    }

    public async Task<PaymentConfigurationDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        if (!await BusinessExistsAsync(businessId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var row = await _db.PaymentConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return row is null
            ? DefaultDto()
            : MapDto(row.Gateway, TryDecryptPayload(row.EncryptedJson));
    }

    public async Task<(PaymentConfigurationDto? Data, PaymentConfigurationUpdateError? Error)> UpdateAsync(
        Guid businessId,
        UpdatePaymentConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        var business = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (business is null)
        {
            return (null, PaymentConfigurationUpdateError.NotFound);
        }

        if (!TryParseProvider(request.Provider, out var newProvider))
        {
            return (null, PaymentConfigurationUpdateError.InvalidRequest);
        }

        var existing = await _db.PaymentConfigurations
            .FirstOrDefaultAsync(p => p.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (newProvider == PaymentGatewayProvider.None)
        {
            if (existing is not null)
            {
                _db.PaymentConfigurations.Remove(existing);
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return (DefaultDto(), null);
        }

        var existingPayload = existing is not null && existing.Gateway == newProvider
            ? TryDecryptPayload(existing.EncryptedJson)
            : new PaymentGatewayCredentialPayload();

        var merged = existingPayload.MergeIncoming(
            newProvider,
            request.SecretKey,
            request.ApiKey,
            request.ContractCode);

        if (!ValidatePayload(newProvider, merged) || !merged.IsCompleteFor(newProvider))
        {
            return (null, PaymentConfigurationUpdateError.InvalidRequest);
        }

        var plainJson = merged.Serialize();
        var encrypted = _crypto.Encrypt(plainJson);
        var now = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            existing = new PaymentConfiguration
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = businessId,
                Gateway = newProvider,
                EncryptedJson = encrypted,
                CreatedAt = now,
            };
            _db.PaymentConfigurations.Add(existing);
        }
        else
        {
            existing.Gateway = newProvider;
            existing.EncryptedJson = encrypted;
            existing.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (MapDto(newProvider, merged), null);
    }

    public async Task<PaymentGatewayCredentialsDto?> GetDecryptedCredentialsAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.PaymentConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (row is null || row.Gateway == PaymentGatewayProvider.None)
        {
            return null;
        }

        var payload = TryDecryptPayload(row.EncryptedJson);
        if (!payload.IsCompleteFor(row.Gateway))
        {
            return null;
        }

        return new PaymentGatewayCredentialsDto
        {
            Provider = row.Gateway.ToString(),
            SecretKey = payload.SecretKey,
            ApiKey = payload.ApiKey,
            ContractCode = payload.ContractCode,
        };
    }

    private PaymentGatewayCredentialPayload TryDecryptPayload(string encryptedJson)
    {
        try
        {
            var plain = _crypto.Decrypt(encryptedJson);
            return PaymentGatewayCredentialPayload.Deserialize(plain);
        }
        catch
        {
            return new PaymentGatewayCredentialPayload();
        }
    }

    private static PaymentConfigurationDto DefaultDto()
    {
        return new PaymentConfigurationDto
        {
            Provider = PaymentGatewayProvider.None.ToString(),
            IsConfigured = false,
            HasSecretKey = false,
            HasApiKey = false,
            HasContractCode = false,
        };
    }

    private static PaymentConfigurationDto MapDto(
        PaymentGatewayProvider gateway,
        PaymentGatewayCredentialPayload payload)
    {
        if (gateway == PaymentGatewayProvider.None)
        {
            return DefaultDto();
        }

        return new PaymentConfigurationDto
        {
            Provider = gateway.ToString(),
            IsConfigured = payload.IsCompleteFor(gateway),
            HasSecretKey = !string.IsNullOrWhiteSpace(payload.SecretKey),
            HasApiKey = !string.IsNullOrWhiteSpace(payload.ApiKey),
            HasContractCode = !string.IsNullOrWhiteSpace(payload.ContractCode),
        };
    }

    private static bool ValidatePayload(PaymentGatewayProvider gateway, PaymentGatewayCredentialPayload payload)
    {
        if (!IsValidOptionalField(payload.SecretKey))
        {
            return false;
        }

        if (gateway == PaymentGatewayProvider.Monify)
        {
            if (!IsValidOptionalField(payload.ApiKey) || !IsValidOptionalField(payload.ContractCode))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidOptionalField(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var len = value.Trim().Length;
        return len >= MinFieldLength && len <= MaxFieldLength;
    }

    private async Task<bool> BusinessExistsAsync(Guid businessId, CancellationToken cancellationToken)
    {
        return await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);
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
