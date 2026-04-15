using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessRegistrationService : IBusinessRegistrationService
{
    private readonly AdminDbContext _db;
    private readonly PasswordHasher<BusinessRegistration> _passwordHasher = new();

    public BusinessRegistrationService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<RegisterBusinessResult> RegisterAsync(
        RegisterBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        var businessName = request.BusinessName?.Trim() ?? string.Empty;
        if (businessName.Length < 2)
        {
            return RegisterBusinessResult.Fail("Validation", "Business name is required (at least 2 characters).");
        }

        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return RegisterBusinessResult.Fail("Validation", "A valid email is required.");
        }

        if (!request.AcceptTerms)
        {
            return RegisterBusinessResult.Fail("AcceptTermsRequired", "You must accept the terms to register.");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            return RegisterBusinessResult.Fail("Validation", "Password must be at least 8 characters.");
        }

        var exists = await _db.BusinessRegistrations
            .AsNoTracking()
            .AnyAsync(r => r.ContactEmail == email, cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            return RegisterBusinessResult.Fail("DuplicateEmail", "An account with this email already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new BusinessRegistration
        {
            Id = Guid.NewGuid(),
            BusinessName = businessName,
            ContactEmail = email,
            HashedPassword = string.Empty,
            IsEmailVerified = false,
            ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone)
                ? null
                : request.ContactPhone.Trim(),
            Status = BusinessRegistrationStatus.Inactive,
            TermsAcceptedAt = now,
            CreatedAt = now,
            Slug = null,
        };

        entity.HashedPassword = _passwordHasher.HashPassword(entity, request.Password);

        _db.BusinessRegistrations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return RegisterBusinessResult.Ok(Map(entity));
    }

    private static BusinessRegistrationDto Map(BusinessRegistration e) =>
        new()
        {
            Id = e.Id,
            BusinessName = e.BusinessName,
            ContactEmail = e.ContactEmail,
            IsEmailVerified = e.IsEmailVerified,
            Status = e.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive",
            CreatedAt = e.CreatedAt,
            TermsAcceptedAt = e.TermsAcceptedAt,
            ContactPhone = e.ContactPhone,
            Slug = e.Slug,
        };
}
