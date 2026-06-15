using System.Text.RegularExpressions;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessSocialProfileService : IBusinessSocialProfileService
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly AdminDbContext _db;

    public BusinessSocialProfileService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<BusinessSocialProfileDto> GetAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessSocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? new BusinessSocialProfileDto() : Map(entity);
    }

    public async Task<(BusinessSocialProfileDto? Data, string? ErrorMessage)> UpdateAsync(
        Guid businessId,
        UpdateBusinessSocialProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryNormalizeOptionalUrl(request.FacebookUrl, out var facebook, out var urlError)
            || !TryNormalizeOptionalUrl(request.InstagramUrl, out var instagram, out urlError)
            || !TryNormalizeOptionalUrl(request.TikTokUrl, out var tikTok, out urlError)
            || !TryNormalizeOptionalUrl(request.XUrl, out var xUrl, out urlError))
        {
            return (null, urlError);
        }

        var contactEmail = NormalizeOptional(request.ContactEmail);
        if (contactEmail is not null && !EmailPattern.IsMatch(contactEmail))
        {
            return (null, "Contact email is not valid.");
        }

        var contactPhone = NormalizeOptional(request.ContactPhone);
        if (contactPhone is { Length: > 32 })
        {
            return (null, "Contact phone must be 32 characters or fewer.");
        }

        var exists = await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            return (null, "Business not found.");
        }

        var entity = await _db.BusinessSocialProfiles
            .FirstOrDefaultAsync(p => p.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;

        if (entity is null)
        {
            entity = new BusinessSocialProfile
            {
                Id = Guid.NewGuid(),
                BusinessRegistrationId = businessId,
                CreatedAt = now,
            };
            _db.BusinessSocialProfiles.Add(entity);
        }

        entity.FacebookUrl = facebook;
        entity.InstagramUrl = instagram;
        entity.TikTokUrl = tikTok;
        entity.XUrl = xUrl;
        entity.ContactEmail = contactEmail;
        entity.ContactPhone = contactPhone;
        entity.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (Map(entity), null);
    }

    internal static BusinessSocialProfileDto Map(BusinessSocialProfile entity) =>
        new()
        {
            FacebookUrl = entity.FacebookUrl,
            InstagramUrl = entity.InstagramUrl,
            TikTokUrl = entity.TikTokUrl,
            XUrl = entity.XUrl,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
        };

    private static bool TryNormalizeOptionalUrl(string? raw, out string? normalized, out string? error)
    {
        normalized = NormalizeOptional(raw);
        error = null;

        if (normalized is null)
        {
            return true;
        }

        if (normalized.Length > 512)
        {
            error = "Social link URLs must be 512 characters or fewer.";
            normalized = null;
            return false;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            error = "Social links must be valid http or https URLs.";
            normalized = null;
            return false;
        }

        normalized = uri.ToString();
        return true;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
