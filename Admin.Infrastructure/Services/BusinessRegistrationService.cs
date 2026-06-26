using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Data.Dtos;
using Shared.Data.Emails;
using Shared.Services.Abstractions;

namespace Admin.Infrastructure.Services;

public sealed class BusinessRegistrationService : IBusinessRegistrationService
{
    private readonly AdminDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly IBusinessEmailVerificationService _emailVerificationService;
    private readonly ILogger<BusinessRegistrationService> _logger;
    private readonly PasswordHasher<BusinessRegistration> _passwordHasher = new();
    private readonly PasswordHasher<UserOrganization> _userPasswordHasher = new();

    public BusinessRegistrationService(
        AdminDbContext db,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer,
        IBusinessEmailVerificationService emailVerificationService,
        ILogger<BusinessRegistrationService> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _emailVerificationService = emailVerificationService;
        _logger = logger;
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

        var firstName = request.FirstName?.Trim() ?? string.Empty;
        if (firstName.Length < 2)
        {
            return RegisterBusinessResult.Fail("Validation", "First name is required (at least 2 characters).");
        }

        var lastName = request.LastName?.Trim() ?? string.Empty;
        if (lastName.Length < 2)
        {
            return RegisterBusinessResult.Fail("Validation", "Last name is required (at least 2 characters).");
        }

        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return RegisterBusinessResult.Fail("Validation", "A valid email is required.");
        }

        var phoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
        if (phoneNumber.Length < 7)
        {
            return RegisterBusinessResult.Fail("Validation", "A valid phone number is required.");
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
            .ConfigureAwait(false)
            || await _db.UserOrganizations
                .AsNoTracking()
                .AnyAsync(u => u.Email == email, cancellationToken)
                .ConfigureAwait(false);

        if (exists)
        {
            return RegisterBusinessResult.Fail("DuplicateEmail", "An account with this email already exists.");
        }

        if (!BusinessSlugHelper.TryValidate(request.Slug, out var slug, out var slugError))
        {
            return RegisterBusinessResult.Fail("Validation", slugError ?? "Invalid hotel slug.");
        }

        if (await _db.BusinessRegistrations
                .AsNoTracking()
                .AnyAsync(r => r.Slug == slug, cancellationToken)
                .ConfigureAwait(false))
        {
            return RegisterBusinessResult.Fail("DuplicateSlug", "This hotel slug is already taken. Choose another.");
        }

        if (!TryParseBusinessType(request.BusinessType, out var businessType))
        {
            return RegisterBusinessResult.Fail("Validation", "Business type must be Hotel or Shortlet.");
        }

        var planCode = string.IsNullOrWhiteSpace(request.PlanCode)
            ? "free"
            : request.PlanCode.Trim().ToLowerInvariant();

        var plan = await _db.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == planCode && p.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (plan is null)
        {
            return RegisterBusinessResult.Fail("Validation", "Select a valid subscription plan.");
        }

        var now = DateTimeOffset.UtcNow;
        var subscriptionExpiresAt = plan.Tier == SubscriptionPlanTier.Free
            ? SubscriptionAccessHelper.ComputeTrialExpiry(now)
            : SubscriptionAccessHelper.ComputeTrialExpiry(now);

        var entity = new BusinessRegistration
        {
            Id = Guid.NewGuid(),
            BusinessName = businessName,
            FirstName = firstName,
            LastName = lastName,
            ContactEmail = email,
            HashedPassword = string.Empty,
            IsEmailVerified = false,
            PhoneNumber = phoneNumber,
            Status = BusinessRegistrationStatus.Inactive,
            TermsAcceptedAt = now,
            CreatedAt = now,
            Slug = slug,
            BusinessType = businessType,
            SubscriptionPlanId = plan.Id,
            SubscriptionExpiresAt = subscriptionExpiresAt,
        };

        entity.HashedPassword = _passwordHasher.HashPassword(entity, request.Password);

        var superAdmin = new UserOrganization
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = entity.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            HashedPassword = string.Empty,
            IsSuperAdmin = true,
            IsDefaultPassword = false,
            HasAllModuleAccess = true,
            HasAllLocationAccess = true,
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = now,
        };
        superAdmin.HashedPassword = _userPasswordHasher.HashPassword(superAdmin, request.Password);

        _db.BusinessRegistrations.Add(entity);
        _db.UserOrganizations.Add(superAdmin);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await SendWelcomeOnboardEmailAsync(entity, cancellationToken).ConfigureAwait(false);

        return RegisterBusinessResult.Ok(Map(entity));
    }

    private async Task SendWelcomeOnboardEmailAsync(BusinessRegistration entity, CancellationToken cancellationToken)
    {
        try
        {
            // keep welcome email flow
            await SendWelcomeEmailInternalAsync(entity, cancellationToken).ConfigureAwait(false);
            // send OTP immediately for the next step (email verification page)
            await _emailVerificationService
                .SendOtpAsync(entity.Id, entity.BusinessName, entity.ContactEmail, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Registration should not fail if email dispatch fails.
            _logger.LogWarning(ex, "Business registered but onboarding email failed for {Email}", entity.ContactEmail);
        }
    }

    private async Task SendWelcomeEmailInternalAsync(BusinessRegistration entity, CancellationToken cancellationToken)
    {
        var html = _templateRenderer.Render(
            "WelcomeOnboard",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["BusinessName"] = entity.BusinessName,
                ["LoginUrl"] = "http://localhost:4200/login",
                ["SupportEmail"] = "info@arkifi.store",
                ["Year"] = DateTime.UtcNow.Year.ToString(),
            });

        var email = new EmailMessage
        {
            ToEmail = entity.ContactEmail,
            ToName = entity.BusinessName,
            Subject = "Welcome to ArkifiHub - Your onboarding starts now",
            HtmlBody = html,
            TextBody = $"Hi {entity.BusinessName}, welcome to ArkifiHub. Sign in at http://localhost:4200/login.",
        };

        await _emailSender.SendAsync(email, cancellationToken).ConfigureAwait(false);
    }

    private static BusinessRegistrationDto Map(BusinessRegistration e) =>
        new()
        {
            Id = e.Id,
            BusinessName = e.BusinessName,
            FirstName = e.FirstName,
            LastName = e.LastName,
            ContactEmail = e.ContactEmail,
            IsEmailVerified = e.IsEmailVerified,
            Status = e.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive",
            CreatedAt = e.CreatedAt,
            TermsAcceptedAt = e.TermsAcceptedAt,
            PhoneNumber = e.PhoneNumber,
            Slug = e.Slug,
            LogoUrl = e.LogoPath,
        };

    private static bool TryParseBusinessType(string? raw, out BusinessType businessType)
    {
        businessType = BusinessType.Hotel;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "hotel" => Assign(BusinessType.Hotel, out businessType),
            "shortlet" or "shortlets" => Assign(BusinessType.Shortlet, out businessType),
            _ => false,
        };
    }

    private static bool Assign(BusinessType value, out BusinessType businessType)
    {
        businessType = value;
        return true;
    }
}
