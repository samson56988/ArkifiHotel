using Admin.Data.Configurations;
using Admin.Data.Entities;
using Admin.Data.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Admin.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<BusinessRegistration> BusinessRegistrations => Set<BusinessRegistration>();
    public DbSet<EmailVerificationOtp> EmailVerificationOtps => Set<EmailVerificationOtp>();
    public DbSet<BusinessLoginOtpChallenge> BusinessLoginOtpChallenges => Set<BusinessLoginOtpChallenge>();
    public DbSet<RevokedBusinessAccessToken> RevokedBusinessAccessTokens => Set<RevokedBusinessAccessToken>();
    public DbSet<BusinessPasswordResetChallenge> BusinessPasswordResetChallenges => Set<BusinessPasswordResetChallenge>();
    public DbSet<BusinessLocation> BusinessLocations => Set<BusinessLocation>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    public DbSet<RoomAmenity> RoomAmenities => Set<RoomAmenity>();
    public DbSet<PropertyFacility> PropertyFacilities => Set<PropertyFacility>();
    public DbSet<PropertyFacilityImage> PropertyFacilityImages => Set<PropertyFacilityImage>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<BookingPayment> BookingPayments => Set<BookingPayment>();
    public DbSet<PaymentConfiguration> PaymentConfigurations => Set<PaymentConfiguration>();
    public DbSet<BusinessSocialProfile> BusinessSocialProfiles => Set<BusinessSocialProfile>();
    public DbSet<StorefrontBannerImage> StorefrontBannerImages => Set<StorefrontBannerImage>();
    public DbSet<StorefrontAboutImage> StorefrontAboutImages => Set<StorefrontAboutImage>();
    public DbSet<RestaurantMenuSettings> RestaurantMenuSettings => Set<RestaurantMenuSettings>();
    public DbSet<RestaurantMenuCategory> RestaurantMenuCategories => Set<RestaurantMenuCategory>();
    public DbSet<RestaurantMenuItem> RestaurantMenuItems => Set<RestaurantMenuItem>();
    public DbSet<RestaurantOrder> RestaurantOrders => Set<RestaurantOrder>();
    public DbSet<RestaurantOrderLine> RestaurantOrderLines => Set<RestaurantOrderLine>();
    public DbSet<RestaurantOrderPayment> RestaurantOrderPayments => Set<RestaurantOrderPayment>();
    public DbSet<EventHall> EventHalls => Set<EventHall>();
    public DbSet<EventHallImage> EventHallImages => Set<EventHallImage>();
    public DbSet<EventHallRequest> EventHallRequests => Set<EventHallRequest>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<BusinessSubscriptionPayment> BusinessSubscriptionPayments => Set<BusinessSubscriptionPayment>();
    public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
    public DbSet<UserOrganizationModulePermission> UserOrganizationModulePermissions =>
        Set<UserOrganizationModulePermission>();
    public DbSet<UserOrganizationLocationPermission> UserOrganizationLocationPermissions =>
        Set<UserOrganizationLocationPermission>();
    public DbSet<OrganizationAuditLog> OrganizationAuditLogs => Set<OrganizationAuditLog>();
    public DbSet<PlatformStaff> PlatformStaff => Set<PlatformStaff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BusinessRegistrationConfiguration());
        modelBuilder.ApplyConfiguration(new EmailVerificationOtpConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessLoginOtpChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new RevokedBusinessAccessTokenConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessPasswordResetChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessLocationConfiguration());
        modelBuilder.ApplyConfiguration(new AmenityConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new RoomImageConfiguration());
        modelBuilder.ApplyConfiguration(new RoomAmenityConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyFacilityConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyFacilityImageConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new BookingPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessSocialProfileConfiguration());
        modelBuilder.ApplyConfiguration(new StorefrontBannerImageConfiguration());
        modelBuilder.ApplyConfiguration(new StorefrontAboutImageConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantMenuSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantMenuCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantMenuItemConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantOrderConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantOrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantOrderPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new EventHallConfiguration());
        modelBuilder.ApplyConfiguration(new EventHallImageConfiguration());
        modelBuilder.ApplyConfiguration(new EventHallRequestConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessSubscriptionPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganizationConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganizationModulePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganizationLocationPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationAuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new PlatformStaffConfiguration());
        DefaultAmenitySeed.Apply(modelBuilder);
        SubscriptionPlanSeed.Apply(modelBuilder);
    }
}
