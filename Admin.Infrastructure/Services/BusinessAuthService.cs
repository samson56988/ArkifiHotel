using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Options;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessAuthService : IBusinessAuthService
{
    private readonly AdminDbContext _db;
    private readonly JwtOptions _jwt;
    private readonly PasswordHasher<BusinessRegistration> _passwordHasher = new();

    public BusinessAuthService(AdminDbContext db, IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _jwt = jwtOptions.Value;
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
        });
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
}
