using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Admin.Data;
using Admin.Data.Constants;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
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
    private readonly PasswordHasher<UserOrganization> _userPasswordHasher = new();

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
        var login = (request.Login ?? request.Email)?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(request.Password))
        {
            return LoginBusinessResult.Fail("Validation", "Sign-in ID and password are required.");
        }

        var (orgUser, entity) = await ResolveLoginAccountAsync(login, cancellationToken).ConfigureAwait(false);

        if (entity is null)
        {
            return LoginBusinessResult.Fail("InvalidCredentials", "Invalid sign-in ID or password.");
        }

        if (orgUser is not null && !orgUser.IsActive)
        {
            return LoginBusinessResult.Fail(
                "AccountBlocked",
                "Your account has been blocked. Contact your business administrator.");
        }

        if (!VerifyLoginPassword(orgUser, entity, request.Password))
        {
            return LoginBusinessResult.Fail("InvalidCredentials", "Invalid sign-in ID or password.");
        }

        var account = await BuildAccountDtoAsync(entity, orgUser, cancellationToken).ConfigureAwait(false);

        if (orgUser is not null && orgUser.IsDefaultPassword)
        {
            return LoginBusinessResult.Ok(new LoginBusinessData
            {
                RequiresPasswordChange = true,
                Account = account,
            });
        }

        var isEmailVerified = orgUser?.IsEmailVerified ?? entity.IsEmailVerified;
        if (!isEmailVerified)
        {
            var verificationEmail = orgUser?.Email ?? entity.ContactEmail;
            try
            {
                await _emailVerificationService
                    .SendOtpAsync(entity.Id, entity.BusinessName, verificationEmail, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed sending re-verification OTP to {Email}", verificationEmail);
            }

            return LoginBusinessResult.Fail(
                "EmailNotVerified",
                "Email not verified. A new OTP has been sent to your inbox.");
        }

        var challenge = await CreateAndSendLoginOtpChallengeAsync(entity, orgUser, cancellationToken)
            .ConfigureAwait(false);
        var twoFactorEmail = orgUser?.Email ?? entity.ContactEmail;

        return LoginBusinessResult.Ok(new LoginBusinessData
        {
            RequiresTwoFactor = true,
            ChallengeId = challenge.Id.ToString(),
            ChallengeExpiresAtUtc = challenge.ExpiresAt,
            Account = account,
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

        var (orgUser, entity) = await ResolveLoginAccountAsync(email, cancellationToken, tracked: true)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return LoginBusinessResult.Fail("InvalidCredentials", "Invalid verification request.");
        }

        if (orgUser is not null && !orgUser.IsActive)
        {
            return LoginBusinessResult.Fail(
                "AccountBlocked",
                "Your account has been blocked. Contact your business administrator.");
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

        var account = await BuildAccountDtoAsync(entity, orgUser, cancellationToken).ConfigureAwait(false);

        var expires = GetExpiryUtc(request.RememberMe);
        var token = CreateJwt(entity, orgUser, account.ModuleCodes, account.LocationIds, account.HasAllLocationAccess, expires);

        return LoginBusinessResult.Ok(new LoginBusinessData
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            Account = account,
            RequiresTwoFactor = false,
        });
    }

    public async Task<ChangeDefaultPasswordResult> ChangeDefaultPasswordAsync(
        ChangeDefaultPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var login = request.Login?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            return ChangeDefaultPasswordResult.Fail("Validation", "Sign-in ID, current password, and new password are required.");
        }

        if (request.NewPassword.Length < 8)
        {
            return ChangeDefaultPasswordResult.Fail("Validation", "New password must be at least 8 characters.");
        }

        var (orgUser, entity) = await ResolveLoginAccountAsync(login, cancellationToken, tracked: true)
            .ConfigureAwait(false);
        if (entity is null || orgUser is null)
        {
            return ChangeDefaultPasswordResult.Fail("InvalidCredentials", "Invalid sign-in ID or password.");
        }

        if (!orgUser.IsActive)
        {
            return ChangeDefaultPasswordResult.Fail(
                "AccountBlocked",
                "Your account has been blocked. Contact your business administrator.");
        }

        if (!orgUser.IsDefaultPassword)
        {
            return ChangeDefaultPasswordResult.Fail("Validation", "This account is not using a temporary password.");
        }

        if (!VerifyLoginPassword(orgUser, entity, request.CurrentPassword))
        {
            return ChangeDefaultPasswordResult.Fail("InvalidCredentials", "Invalid sign-in ID or password.");
        }

        orgUser.HashedPassword = _userPasswordHasher.HashPassword(orgUser, request.NewPassword);
        orgUser.IsDefaultPassword = false;
        orgUser.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ChangeDefaultPasswordResult.Ok();
    }

    private async Task<(UserOrganization? User, BusinessRegistration? Business)> ResolveLoginAccountAsync(
        string loginIdentifier,
        CancellationToken cancellationToken,
        bool tracked = false)
    {
        var trimmed = loginIdentifier.Trim();

        if (trimmed.Contains('@', StringComparison.Ordinal) && !trimmed.Contains('/'))
        {
            return await ResolveByEmailAsync(trimmed.ToLowerInvariant(), cancellationToken, tracked)
                .ConfigureAwait(false);
        }

        if (OrganizationUsernameHelper.TryParseStaffLogin(trimmed, out var slug, out var username, out _))
        {
            return await ResolveByStaffLoginAsync(slug, username, cancellationToken, tracked).ConfigureAwait(false);
        }

        return (null, null);
    }

    private async Task<(UserOrganization? User, BusinessRegistration? Business)> ResolveByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken,
        bool tracked)
    {
        IQueryable<UserOrganization> userQuery = tracked ? _db.UserOrganizations : _db.UserOrganizations.AsNoTracking();
        var orgUser = await userQuery
            .Include(u => u.ModulePermissions)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (orgUser is not null)
        {
            IQueryable<BusinessRegistration> businessQuery = tracked
                ? _db.BusinessRegistrations
                : _db.BusinessRegistrations.AsNoTracking();
            var business = await businessQuery
                .FirstOrDefaultAsync(b => b.Id == orgUser.BusinessRegistrationId, cancellationToken)
                .ConfigureAwait(false);
            return (orgUser, business);
        }

        IQueryable<BusinessRegistration> legacyQuery = tracked
            ? _db.BusinessRegistrations
            : _db.BusinessRegistrations.AsNoTracking();
        var legacy = await legacyQuery
            .FirstOrDefaultAsync(r => r.ContactEmail == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);
        return (null, legacy);
    }

    private async Task<(UserOrganization? User, BusinessRegistration? Business)> ResolveByStaffLoginAsync(
        string businessSlug,
        string username,
        CancellationToken cancellationToken,
        bool tracked)
    {
        IQueryable<UserOrganization> userQuery = tracked ? _db.UserOrganizations : _db.UserOrganizations.AsNoTracking();
        var orgUser = await userQuery
            .Include(u => u.ModulePermissions)
            .Include(u => u.BusinessRegistration)
            .FirstOrDefaultAsync(
                u => u.Username == username
                    && !u.IsSuperAdmin
                    && u.BusinessRegistration.Slug == businessSlug,
                cancellationToken)
            .ConfigureAwait(false);

        return (orgUser, orgUser?.BusinessRegistration);
    }

    private bool VerifyLoginPassword(UserOrganization? orgUser, BusinessRegistration business, string password)
    {
        if (orgUser is not null)
        {
            var verify = _userPasswordHasher.VerifyHashedPassword(orgUser, orgUser.HashedPassword, password);
            return verify != PasswordVerificationResult.Failed;
        }

        var legacyVerify = _passwordHasher.VerifyHashedPassword(business, business.HashedPassword, password);
        return legacyVerify != PasswordVerificationResult.Failed;
    }

    private async Task<BusinessAccountDto> BuildAccountDtoAsync(
        BusinessRegistration entity,
        UserOrganization? orgUser,
        CancellationToken cancellationToken)
    {
        if (orgUser is not null && !orgUser.ModulePermissions.Any())
        {
            await _db.Entry(orgUser)
                .Collection(u => u.ModulePermissions)
                .LoadAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        if (orgUser is not null && !orgUser.LocationPermissions.Any())
        {
            await _db.Entry(orgUser)
                .Collection(u => u.LocationPermissions)
                .LoadAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        var isShortlet = entity.BusinessType == BusinessType.Shortlet;
        var modules = orgUser is null
            ? OrganizationModuleCodes.ForBusinessType(isShortlet)
                .Append(OrganizationModuleCodes.Team)
                .Append(OrganizationModuleCodes.Audit)
                .ToList()
            : OrganizationAccessHelper.ResolveModuleCodes(orgUser, isShortlet);

        var hasAllLocations = orgUser is null || OrganizationLocationHelper.HasAllLocationAccess(orgUser);
        var locationIds = orgUser is null
            ? Array.Empty<Guid>()
            : OrganizationLocationHelper.ResolveLocationIds(orgUser);

        return new BusinessAccountDto
        {
            Id = entity.Id,
            BusinessName = entity.BusinessName,
            ContactEmail = orgUser?.Email ?? entity.ContactEmail,
            IsEmailVerified = orgUser?.IsEmailVerified ?? entity.IsEmailVerified,
            Status = entity.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive",
            UserId = orgUser?.Id,
            FirstName = orgUser?.FirstName ?? entity.FirstName,
            LastName = orgUser?.LastName ?? entity.LastName,
            IsSuperAdmin = orgUser?.IsSuperAdmin ?? true,
            Username = orgUser?.Username,
            HasAllModuleAccess = orgUser?.IsSuperAdmin == true || orgUser?.HasAllModuleAccess == true,
            HasAllLocationAccess = hasAllLocations,
            DefaultLocationId = orgUser?.DefaultLocationId,
            RequiresPasswordChange = orgUser?.IsDefaultPassword == true,
            TwoFactorEmail = orgUser?.Email ?? entity.ContactEmail,
            ModuleCodes = modules,
            LocationIds = locationIds,
        };
    }

    private async Task<BusinessLoginOtpChallenge> CreateAndSendLoginOtpChallengeAsync(
        BusinessRegistration entity,
        UserOrganization? orgUser,
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

        var recipientEmail = orgUser?.Email ?? entity.ContactEmail;
        var recipientName = orgUser is null ? entity.BusinessName : $"{orgUser.FirstName} {orgUser.LastName}";

        var email = new EmailMessage
        {
            ToEmail = recipientEmail,
            ToName = recipientName,
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

    private string CreateJwt(
        BusinessRegistration entity,
        UserOrganization? orgUser,
        IReadOnlyList<string> moduleCodes,
        IReadOnlyList<Guid> locationIds,
        bool hasAllLocationAccess,
        DateTimeOffset expiresUtc)
    {
        if (string.IsNullOrWhiteSpace(_jwt.Secret) || _jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be configured and at least 32 characters long.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var email = orgUser?.Email ?? entity.ContactEmail;
        var isSuperAdmin = orgUser?.IsSuperAdmin ?? true;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, entity.Id.ToString()),
            new("business_id", entity.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("business_name", entity.BusinessName),
            new(ClaimTypes.Role, "Business"),
            new("status", entity.Status == BusinessRegistrationStatus.Active ? "Active" : "Inactive"),
            new("is_super_admin", isSuperAdmin ? "true" : "false"),
        };

        if (orgUser is not null)
        {
            claims.Add(new Claim("user_id", orgUser.Id.ToString()));
            claims.Add(new Claim("given_name", orgUser.FirstName));
            claims.Add(new Claim("family_name", orgUser.LastName));
            if (!string.IsNullOrWhiteSpace(orgUser.Username))
            {
                claims.Add(new Claim("username", orgUser.Username));
            }
        }

        if (moduleCodes.Count > 0)
        {
            claims.Add(new Claim("modules", string.Join(',', moduleCodes)));
        }

        if (hasAllLocationAccess)
        {
            claims.Add(new Claim("all_locations", "true"));
        }
        else if (locationIds.Count > 0)
        {
            claims.Add(new Claim("location_ids", string.Join(',', locationIds)));
        }

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
