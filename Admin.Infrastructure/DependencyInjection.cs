using Admin.Data;
using Admin.Infrastructure.Options;
using Admin.Infrastructure.Services;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Adds PostgreSQL <see cref="AdminDbContext"/> and admin domain services.</summary>
    /// <param name="connectionStringName">Preferred configuration key under ConnectionStrings; falls back to DefaultConnection.</param>
    public static IServiceCollection AddAdminInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "AdminDb")
    {
        var cs = configuration.GetConnectionString(connectionStringName)
                 ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                "A PostgreSQL connection string is required. Set ConnectionStrings:AdminDb or ConnectionStrings:DefaultConnection.");
        }

        services.AddDbContext<AdminDbContext>(options =>
            options.UseNpgsql(cs));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IBusinessRegistrationService, BusinessRegistrationService>();
        services.AddScoped<IBusinessAuthService, BusinessAuthService>();

        return services;
    }
}
