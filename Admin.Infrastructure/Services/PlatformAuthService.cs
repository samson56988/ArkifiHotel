using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Infrastructure.Options;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class PlatformAuthService : IPlatformAuthService
{
    private readonly AdminDbContext _db;
    private readonly JwtOptions _jwt;
    private readonly PasswordHasher<PlatformStaff> _passwordHasher = new();

    public PlatformAuthService(AdminDbContext db, IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _jwt = jwtOptions.Value;
    }

    public async Task<PlatformLoginResult> LoginAsync(
        PlatformLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(request.Password))
        {
            return PlatformLoginResult.Fail("Validation", "Email and password are required.");
        }

        var staff = await _db.PlatformStaff
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken)
            .ConfigureAwait(false);

        if (staff is null || !staff.IsActive)
        {
            return PlatformLoginResult.Fail("InvalidCredentials", "Invalid email or password.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(staff, staff.HashedPassword, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return PlatformLoginResult.Fail("InvalidCredentials", "Invalid email or password.");
        }

        var expiresUtc = DateTimeOffset.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var token = CreateAccessToken(staff, expiresUtc);
        var account = new PlatformStaffAccountDto
        {
            Id = staff.Id,
            Email = staff.Email,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            DisplayName = $"{staff.FirstName} {staff.LastName}".Trim(),
        };

        return PlatformLoginResult.Ok(new PlatformLoginData
        {
            AccessToken = token,
            ExpiresAtUtc = expiresUtc,
            Account = account,
        });
    }

    private string CreateAccessToken(PlatformStaff staff, DateTimeOffset expiresUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
            new("staff_id", staff.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, staff.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.GivenName, staff.FirstName),
            new(ClaimTypes.Surname, staff.LastName),
            new(ClaimTypes.Role, "Platform"),
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
