using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessProfileService : IBusinessProfileService
{
    private readonly AdminDbContext _db;

    public BusinessProfileService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<BusinessProfileDto?> GetAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        return entity is null ? null : Map(entity);
    }

    public async Task<(BusinessProfileDto? Data, BusinessProfileUpdateError Error, string? Message)> UpdateAsync(
        Guid businessId,
        UpdateBusinessProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var businessName = request.BusinessName?.Trim() ?? string.Empty;
        if (businessName.Length < 2)
        {
            return (null, BusinessProfileUpdateError.Validation, "Business name is required (at least 2 characters).");
        }

        if (!BusinessSlugHelper.TryValidate(request.Slug, out var slug, out var slugError))
        {
            return (null, BusinessProfileUpdateError.Validation, slugError);
        }

        var entity = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return (null, BusinessProfileUpdateError.NotFound, "Business not found.");
        }

        if (await SlugTakenAsync(slug, excludeBusinessId: businessId, cancellationToken).ConfigureAwait(false))
        {
            return (null, BusinessProfileUpdateError.DuplicateSlug, "This hotel slug is already taken. Choose another.");
        }

        entity.BusinessName = businessName;
        entity.Slug = slug;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (Map(entity), BusinessProfileUpdateError.None, null);
    }

    public async Task<(BusinessProfileDto? Data, BusinessProfileUpdateError Error, string? Message)> UpdateLogoPathAsync(
        Guid businessId,
        string? relativeLogoPath,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return (null, BusinessProfileUpdateError.NotFound, "Business not found.");
        }

        entity.LogoPath = string.IsNullOrWhiteSpace(relativeLogoPath) ? null : relativeLogoPath.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return (Map(entity), BusinessProfileUpdateError.None, null);
    }

    public async Task<SlugAvailabilityDto> CheckSlugAvailabilityAsync(
        string slug,
        Guid? excludeBusinessId = null,
        CancellationToken cancellationToken = default)
    {
        if (!BusinessSlugHelper.TryValidate(slug, out var normalized, out _))
        {
            return new SlugAvailabilityDto { Slug = BusinessSlugHelper.Normalize(slug), Available = false };
        }

        var taken = await SlugTakenAsync(normalized, excludeBusinessId, cancellationToken).ConfigureAwait(false);
        return new SlugAvailabilityDto { Slug = normalized, Available = !taken };
    }

    internal static BusinessProfileDto Map(BusinessRegistration entity) =>
        new()
        {
            Id = entity.Id,
            BusinessName = entity.BusinessName,
            Slug = entity.Slug,
            LogoUrl = entity.LogoPath,
            ContactEmail = entity.ContactEmail,
            PhoneNumber = entity.PhoneNumber,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            IsEmailVerified = entity.IsEmailVerified,
            Status = entity.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive",
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            BusinessType = entity.BusinessType == BusinessType.Shortlet ? "Shortlet" : "Hotel",
        };

    private async Task<bool> SlugTakenAsync(string slug, Guid? excludeBusinessId, CancellationToken cancellationToken)
    {
        return await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(
                b => b.Slug == slug && (!excludeBusinessId.HasValue || b.Id != excludeBusinessId.Value),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
