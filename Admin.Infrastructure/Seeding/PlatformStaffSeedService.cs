using Admin.Data;
using Admin.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Seeding;

/// <summary>Creates a default platform staff account in development when none exist.</summary>
public sealed class PlatformStaffSeedService
{
    public const string DefaultEmail = "admin@arkifistay.com";
    public const string DefaultPassword = "ArkifiAdmin2026!";

    private readonly AdminDbContext _db;
    private readonly ILogger<PlatformStaffSeedService> _logger;
    private readonly PasswordHasher<PlatformStaff> _passwordHasher = new();

    public PlatformStaffSeedService(AdminDbContext db, ILogger<PlatformStaffSeedService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedDefaultStaffAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.PlatformStaff.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var staff = new PlatformStaff
        {
            Id = Guid.NewGuid(),
            Email = DefaultEmail,
            FirstName = "Platform",
            LastName = "Admin",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        staff.HashedPassword = _passwordHasher.HashPassword(staff, DefaultPassword);

        _db.PlatformStaff.Add(staff);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Seeded default platform staff {Email}. Change the password after first login.",
            DefaultEmail);
    }
}
