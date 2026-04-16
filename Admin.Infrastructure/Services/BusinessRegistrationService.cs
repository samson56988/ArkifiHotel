using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
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
            FirstName = firstName,
            LastName = lastName,
            ContactEmail = email,
            HashedPassword = string.Empty,
            IsEmailVerified = false,
            PhoneNumber = phoneNumber,
            Status = BusinessRegistrationStatus.Inactive,
            TermsAcceptedAt = now,
            CreatedAt = now,
            Slug = null,
        };

        entity.HashedPassword = _passwordHasher.HashPassword(entity, request.Password);

        _db.BusinessRegistrations.Add(entity);
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
        };
}
