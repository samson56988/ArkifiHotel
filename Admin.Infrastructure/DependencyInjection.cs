using Admin.Data;
using Admin.Infrastructure.Options;
using Admin.Infrastructure.Services;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.DataProtection;
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
        services.AddDataProtection()
            .SetApplicationName("ArkifiHotel");

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
        services.Configure<EncryptionSettings>(configuration.GetSection(EncryptionSettings.SectionName));

        services.AddScoped<IBusinessEmailVerificationService, BusinessEmailVerificationService>();
        services.AddScoped<IBusinessRegistrationService, BusinessRegistrationService>();
        services.AddScoped<IBusinessProfileService, BusinessProfileService>();
        services.AddScoped<IStorefrontThemeService, StorefrontThemeService>();
        services.AddScoped<IStorefrontBannerImageService, StorefrontBannerImageService>();
        services.AddScoped<IStorefrontAboutImageService, StorefrontAboutImageService>();
        services.AddScoped<IBusinessSocialProfileService, BusinessSocialProfileService>();
        services.AddScoped<IBusinessAuthService, BusinessAuthService>();
        services.AddScoped<IBusinessPasswordResetService, BusinessPasswordResetService>();
        services.AddScoped<IBusinessAmenityService, BusinessAmenityService>();
        services.AddScoped<IBusinessLocationService, BusinessLocationService>();
        services.AddScoped<IBusinessRoomService, BusinessRoomService>();
        services.AddScoped<IBusinessPropertyFacilityService, BusinessPropertyFacilityService>();
        services.AddScoped<IBusinessBookingService, BusinessBookingService>();
        services.AddScoped<IPublicBookingLookupService, PublicBookingLookupService>();
        services.AddSingleton<IConfigurationEncryptionService, ConfigurationEncryptionService>();
        services.AddScoped<IBusinessPaymentConfigurationService, BusinessPaymentConfigurationService>();
        services.AddScoped<IBusinessCustomerService, BusinessCustomerService>();
        services.AddScoped<IBusinessBookingPaymentService, BusinessBookingPaymentService>();

        return services;
    }
}
