using System.Security.Cryptography;
using System.Text;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Data.Dtos;
using Shared.Data.Emails;
using Shared.Services.Abstractions;

namespace Admin.Infrastructure.Services;

public sealed class BusinessPasswordResetService : IBusinessPasswordResetService
{
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 15;
    private const int ActiveOtpWindowMinutes = 30;

    private const string GenericRequestMessage =
        "If an account exists for this email, we sent a 6-digit reset code. Check your inbox.";

    private readonly AdminDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<BusinessPasswordResetService> _logger;
    private readonly PasswordHasher<BusinessRegistration> _passwordHasher = new();
    private readonly PasswordHasher<UserOrganization> _userPasswordHasher = new();

    public BusinessPasswordResetService(
        AdminDbContext db,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer,
        ILogger<BusinessPasswordResetService> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task<RequestPasswordResetResult> RequestResetAsync(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return RequestPasswordResetResult.Fail("Validation", "A valid email is required.");
        }

        var (orgUser, entity) = await ResolveAccountByEmailAsync(email, cancellationToken).ConfigureAwait(false);

        if (entity is null)
        {
            return RequestPasswordResetResult.Ok(null, GenericRequestMessage);
        }

        var now = DateTimeOffset.UtcNow;
        var stale = await _db.BusinessPasswordResetChallenges
            .Where(x =>
                x.BusinessRegistrationId == entity.Id &&
                !x.IsUsed &&
                x.CreatedAt < now.AddMinutes(-ActiveOtpWindowMinutes))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var otp in stale)
        {
            otp.IsUsed = true;
            otp.UsedAt = now;
        }

        var otpCode = GenerateOtpCode(OtpLength);
        var challenge = new BusinessPasswordResetChallenge
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = entity.Id,
            OtpCodeHash = ComputeSha256(otpCode),
            ExpiresAt = now.AddMinutes(OtpExpiryMinutes),
            IsUsed = false,
            CreatedAt = now,
        };

        _db.BusinessPasswordResetChallenges.Add(challenge);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var html = _templateRenderer.Render(
                "PasswordResetOtp",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["BusinessName"] = entity.BusinessName,
                    ["OtpCode"] = otpCode,
                    ["ExpiresMinutes"] = OtpExpiryMinutes.ToString(),
                    ["SupportEmail"] = "info@arkifi.store",
                    ["Year"] = DateTime.UtcNow.Year.ToString(),
                });

            var emailMessage = new EmailMessage
            {
                ToEmail = orgUser?.Email ?? entity.ContactEmail,
                ToName = entity.BusinessName,
                Subject = "Reset your ArkifiHub password",
                HtmlBody = html,
                TextBody =
                    $"Your ArkifiHub password reset code is {otpCode}. It expires in {OtpExpiryMinutes} minutes.",
            };

            await _emailSender.SendAsync(emailMessage, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send password reset OTP to {Email}", entity.ContactEmail);
            return RequestPasswordResetResult.Fail(
                "EmailSendFailed",
                "We could not send the reset email. Try again in a few minutes.");
        }

        return RequestPasswordResetResult.Ok(
            new RequestPasswordResetData
            {
                ChallengeId = challenge.Id.ToString(),
                ChallengeExpiresAtUtc = challenge.ExpiresAt,
            },
            GenericRequestMessage);
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var otp = request.Otp?.Trim() ?? string.Empty;
        var newPassword = request.NewPassword ?? string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(request.ChallengeId))
        {
            return ResetPasswordResult.Fail("Validation", "Email, OTP, and challenge are required.");
        }

        if (newPassword.Length < 8)
        {
            return ResetPasswordResult.Fail("Validation", "Password must be at least 8 characters.");
        }

        if (!Guid.TryParse(request.ChallengeId, out var challengeId))
        {
            return ResetPasswordResult.Fail("Validation", "Invalid reset challenge.");
        }

        var (orgUser, entity) = await ResolveAccountByEmailAsync(email, cancellationToken, tracked: true)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return ResetPasswordResult.Fail("InvalidOtp", "Invalid or expired reset code.");
        }

        var now = DateTimeOffset.UtcNow;
        var hashed = ComputeSha256(otp);
        var challenge = await _db.BusinessPasswordResetChallenges
            .FirstOrDefaultAsync(
                x => x.Id == challengeId
                    && x.BusinessRegistrationId == entity.Id
                    && !x.IsUsed
                    && x.ExpiresAt >= now,
                cancellationToken)
            .ConfigureAwait(false);

        if (challenge is null || !string.Equals(challenge.OtpCodeHash, hashed, StringComparison.Ordinal))
        {
            return ResetPasswordResult.Fail("InvalidOtp", "Invalid or expired reset code.");
        }

        challenge.IsUsed = true;
        challenge.UsedAt = now;
        entity.HashedPassword = _passwordHasher.HashPassword(entity, newPassword);
        entity.UpdatedAt = now;

        if (orgUser is not null)
        {
            orgUser.HashedPassword = _userPasswordHasher.HashPassword(orgUser, newPassword);
            orgUser.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ResetPasswordResult.Ok();
    }

    private async Task<(UserOrganization? User, BusinessRegistration? Business)> ResolveAccountByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken,
        bool tracked = false)
    {
        var userQuery = tracked ? _db.UserOrganizations : _db.UserOrganizations.AsNoTracking();
        var orgUser = await userQuery
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (orgUser is not null)
        {
            var businessQuery = tracked ? _db.BusinessRegistrations : _db.BusinessRegistrations.AsNoTracking();
            var business = await businessQuery
                .FirstOrDefaultAsync(b => b.Id == orgUser.BusinessRegistrationId, cancellationToken)
                .ConfigureAwait(false);
            return (orgUser, business);
        }

        var legacyQuery = tracked ? _db.BusinessRegistrations : _db.BusinessRegistrations.AsNoTracking();
        var legacy = await legacyQuery
            .FirstOrDefaultAsync(r => r.ContactEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        return (null, legacy);
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
