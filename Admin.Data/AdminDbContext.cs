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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BusinessRegistrationConfiguration());
        modelBuilder.ApplyConfiguration(new EmailVerificationOtpConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessLoginOtpChallengeConfiguration());
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
        DefaultAmenitySeed.Apply(modelBuilder);
    }
}
