using System.Security.Cryptography;
using System.Text;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Data.Dtos;
using Shared.Data.Emails;
using Shared.Services.Abstractions;

namespace Admin.Infrastructure.Services;

public sealed class BusinessEmailVerificationService : IBusinessEmailVerificationService
{
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 10;
    private const int ActiveOtpWindowMinutes = 30;

    private readonly AdminDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<BusinessEmailVerificationService> _logger;

    public BusinessEmailVerificationService(
        AdminDbContext db,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer,
        ILogger<BusinessEmailVerificationService> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task SendOtpAsync(
        Guid businessId,
        string businessName,
        string contactEmail,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var stalePendingOtps = await _db.EmailVerificationOtps
            .Where(x =>
                x.BusinessRegistrationId == businessId &&
                !x.IsUsed &&
                x.CreatedAt < now.AddMinutes(-ActiveOtpWindowMinutes))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (stalePendingOtps.Count > 0)
        {
            foreach (var otp in stalePendingOtps)
            {
                otp.IsUsed = true;
                otp.UsedAt = now;
            }
        }

        var otpCode = GenerateOtpCode(OtpLength);
        var entity = new EmailVerificationOtp
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            OtpCodeHash = ComputeSha256(otpCode),
            ExpiresAt = now.AddMinutes(OtpExpiryMinutes),
            IsUsed = false,
            CreatedAt = now,
        };

        _db.EmailVerificationOtps.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var html = _templateRenderer.Render(
                "EmailVerificationOtp",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["BusinessName"] = businessName,
                    ["OtpCode"] = otpCode,
                    ["ExpiresMinutes"] = OtpExpiryMinutes.ToString(),
                    ["SupportEmail"] = "info@arkifi.store",
                    ["Year"] = DateTime.UtcNow.Year.ToString(),
                });

            var email = new EmailMessage
            {
                ToEmail = contactEmail,
                ToName = businessName,
                Subject = "Verify your ArkifiHub email address",
                HtmlBody = html,
                TextBody = $"Your ArkifiHub verification code is {otpCode}. It expires in {OtpExpiryMinutes} minutes.",
            };

            await _emailSender.SendAsync(email, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send OTP email to {Email}", contactEmail);
            throw;
        }
    }

    public async Task<VerifyEmailOtpResult> VerifyOtpAsync(
        VerifyEmailOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var otp = request.Otp?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
        {
            return VerifyEmailOtpResult.Fail("Validation", "Email and OTP are required.");
        }

        var business = await _db.BusinessRegistrations
            .FirstOrDefaultAsync(x => x.ContactEmail == email, cancellationToken)
            .ConfigureAwait(false);

        UserOrganization? orgUser = null;
        if (business is null)
        {
            orgUser = await _db.UserOrganizations
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
                .ConfigureAwait(false);
            if (orgUser is null)
            {
                return VerifyEmailOtpResult.Fail("NotFound", "Business account not found.");
            }

            business = await _db.BusinessRegistrations
                .FirstOrDefaultAsync(b => b.Id == orgUser.BusinessRegistrationId, cancellationToken)
                .ConfigureAwait(false);
            if (business is null)
            {
                return VerifyEmailOtpResult.Fail("NotFound", "Business account not found.");
            }
        }
        else
        {
            orgUser = await _db.UserOrganizations
                .FirstOrDefaultAsync(
                    u => u.BusinessRegistrationId == business.Id && u.Email == email,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (business.IsEmailVerified && (orgUser is null || orgUser.IsEmailVerified))
        {
            return VerifyEmailOtpResult.Ok();
        }

        var now = DateTimeOffset.UtcNow;
        var hashed = ComputeSha256(otp);

        var otpEntity = await _db.EmailVerificationOtps
            .Where(x =>
                x.BusinessRegistrationId == business.Id &&
                !x.IsUsed &&
                x.ExpiresAt >= now)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.OtpCodeHash == hashed, cancellationToken)
            .ConfigureAwait(false);

        if (otpEntity is null)
        {
            return VerifyEmailOtpResult.Fail("InvalidOtp", "Invalid or expired OTP.");
        }

        otpEntity.IsUsed = true;
        otpEntity.UsedAt = now;

        business.IsEmailVerified = true;
        business.Status = BusinessRegistrationStatus.Active;
        business.UpdatedAt = now;

        if (orgUser is not null)
        {
            orgUser.IsEmailVerified = true;
            orgUser.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return VerifyEmailOtpResult.Ok();
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
