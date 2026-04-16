using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Options;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Data.Dtos;
using Shared.Data.Emails;
using Shared.Services.Abstractions;

namespace Admin.Infrastructure.Services;

public sealed class BusinessAuthService : IBusinessAuthService
{
    private const int LoginOtpLength = 6;
    private const int LoginOtpExpiryMinutes = 10;

    private readonly AdminDbContext _db;
    private readonly JwtOptions _jwt;
    private readonly IBusinessEmailVerificationService _emailVerificationService;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<BusinessAuthService> _logger;
    private readonly PasswordHasher<BusinessRegistration> _passwordHasher = new();

    public BusinessAuthService(
        AdminDbContext db,
        IOptions<JwtOptions> jwtOptions,
        IBusinessEmailVerificationService emailVerificationService,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer,
        ILogger<BusinessAuthService> logger)
    {
        _db = db;
        _jwt = jwtOptions.Value;
        _emailVerificationService = emailVerificationService;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task<LoginBusinessResult> LoginAsync(
        LoginBusinessRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(request.Password))
        {
            return LoginBusinessResult.Fail("Validation", "Email and password are required.");
        }

        var entity = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ContactEmail == email, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return LoginBusinessResult.Fail("InvalidCredentials", "Invalid email or password.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(entity, entity.HashedPassword, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return LoginBusinessResult.Fail("InvalidCredentials", "Invalid email or password.");
        }

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // Optional: schedule rehash on next authenticated request; login still succeeds.
        }

        if (!entity.IsEmailVerified)
        {
            try
            {
                await _emailVerificationService
                    .SendOtpAsync(entity.Id, entity.BusinessName, entity.ContactEmail, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed sending re-verification OTP to {Email}", entity.ContactEmail);
            }

            return LoginBusinessResult.Fail(
                "EmailNotVerified",
                "Email not verified. A new OTP has been sent to your inbox.");
        }

        var challenge = await CreateAndSendLoginOtpChallengeAsync(entity, cancellationToken).ConfigureAwait(false);

        return LoginBusinessResult.Ok(new LoginBusinessData
        {
            RequiresTwoFactor = true,
            ChallengeId = challenge.Id.ToString(),
            ChallengeExpiresAtUtc = challenge.ExpiresAt,
        });
    }

    public Task<VerifyEmailOtpResult> VerifyEmailOtpAsync(
        VerifyEmailOtpRequest request,
        CancellationToken cancellationToken = default) =>
        _emailVerificationService.VerifyOtpAsync(request, cancellationToken);

    public async Task<LoginBusinessResult> VerifyLoginOtpAsync(
        VerifyLoginOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var otp = request.Otp?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(request.ChallengeId))
        {
            return LoginBusinessResult.Fail("Validation", "Email, OTP and challenge are required.");
        }

        if (!Guid.TryParse(request.ChallengeId, out var challengeId))
        {
            return LoginBusinessResult.Fail("Validation", "Invalid challenge.");
        }

        var entity = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(r => r.ContactEmail == email, cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return LoginBusinessResult.Fail("InvalidCredentials", "Invalid verification request.");
        }

        var now = DateTimeOffset.UtcNow;
        var hashed = ComputeSha256(otp);
        var challenge = await _db.BusinessLoginOtpChallenges
            .FirstOrDefaultAsync(
                x => x.Id == challengeId
                    && x.BusinessRegistrationId == entity.Id
                    && !x.IsUsed
                    && x.ExpiresAt >= now,
                cancellationToken)
            .ConfigureAwait(false);

        if (challenge is null || !string.Equals(challenge.OtpCodeHash, hashed, StringComparison.Ordinal))
        {
            return LoginBusinessResult.Fail("InvalidOtp", "Invalid or expired OTP.");
        }

        challenge.IsUsed = true;
        challenge.UsedAt = now;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var account = new BusinessAccountDto
        {
            Id = entity.Id,
            BusinessName = entity.BusinessName,
            ContactEmail = entity.ContactEmail,
            IsEmailVerified = entity.IsEmailVerified,
            Status = entity.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive",
        };

        var expires = GetExpiryUtc(request.RememberMe);
        var token = CreateJwt(entity, expires);

        return LoginBusinessResult.Ok(new LoginBusinessData
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            Account = account,
            RequiresTwoFactor = false,
        });
    }

    private async Task<BusinessLoginOtpChallenge> CreateAndSendLoginOtpChallengeAsync(
        BusinessRegistration entity,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var otpCode = GenerateOtpCode(LoginOtpLength);

        var challenge = new BusinessLoginOtpChallenge
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = entity.Id,
            OtpCodeHash = ComputeSha256(otpCode),
            ExpiresAt = now.AddMinutes(LoginOtpExpiryMinutes),
            IsUsed = false,
            CreatedAt = now,
        };

        _db.BusinessLoginOtpChallenges.Add(challenge);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var html = _templateRenderer.Render(
            "LoginTwoFactorOtp",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["BusinessName"] = entity.BusinessName,
                ["OtpCode"] = otpCode,
                ["ExpiresMinutes"] = LoginOtpExpiryMinutes.ToString(),
                ["SupportEmail"] = "info@arkifi.store",
                ["Year"] = DateTime.UtcNow.Year.ToString(),
            });

        var email = new EmailMessage
        {
            ToEmail = entity.ContactEmail,
            ToName = entity.BusinessName,
            Subject = "ArkifiHub login verification code",
            HtmlBody = html,
            TextBody = $"Your ArkifiHub login code is {otpCode}. It expires in {LoginOtpExpiryMinutes} minutes.",
        };

        await _emailSender.SendAsync(email, cancellationToken).ConfigureAwait(false);
        return challenge;
    }

    private DateTimeOffset GetExpiryUtc(bool rememberMe)
    {
        if (rememberMe && _jwt.RememberMeAccessTokenDays > 0)
        {
            return DateTimeOffset.UtcNow.AddDays(_jwt.RememberMeAccessTokenDays);
        }

        return DateTimeOffset.UtcNow.AddMinutes(Math.Max(5, _jwt.AccessTokenMinutes));
    }

    private string CreateJwt(BusinessRegistration entity, DateTimeOffset expiresUtc)
    {
        if (string.IsNullOrWhiteSpace(_jwt.Secret) || _jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be configured and at least 32 characters long.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, entity.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, entity.ContactEmail),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("business_name", entity.BusinessName),
            new(ClaimTypes.Role, "Business"),
            new("status", entity.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive"),
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresUtc.UtcDateTime,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateOtpCode(int digits)
    {
        var min = (int)Math.Pow(10, digits - 1);
        var maxExclusive = (int)Math.Pow(10, digits);
        var value = RandomNumberGenerator.GetInt32(min, maxExclusive);
        return value.ToString();
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
