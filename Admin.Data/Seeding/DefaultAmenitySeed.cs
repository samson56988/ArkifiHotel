using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admin.Data.Seeding;

/// <summary>Platform-wide amenities businesses can pick from (not tied to a business row).</summary>
public static class DefaultAmenitySeed
{
    private static readonly DateTimeOffset SeedTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Amenity>().HasData(
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000001"),
                Name = "Wi-Fi",
                Category = "Connectivity",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000002"),
                Name = "Air conditioning",
                Category = "Climate",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000003"),
                Name = "Smart TV",
                Category = "Entertainment",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000004"),
                Name = "Workspace / desk",
                Category = "Work",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000005"),
                Name = "Private bathroom",
                Category = "Bathroom",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000006"),
                Name = "Hot tub / bathtub",
                Category = "Bathroom",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000007"),
                Name = "Balcony / terrace",
                Category = "Outdoor",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000008"),
                Name = "Kitchenette",
                Category = "Kitchen",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000009"),
                Name = "Coffee machine",
                Category = "Kitchen",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-00000000000a"),
                Name = "Minibar",
                Category = "Convenience",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-00000000000b"),
                Name = "In-room safe",
                Category = "Security",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-00000000000c"),
                Name = "Blackout curtains",
                Category = "Comfort",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-00000000000d"),
                Name = "Parking",
                Category = "Services",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-00000000000e"),
                Name = "Pool access",
                Category = "Facility",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-00000000000f"),
                Name = "Gym access",
                Category = "Facility",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000010"),
                Name = "Room service",
                Category = "Services",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000011"),
                Name = "Hair dryer",
                Category = "Bathroom",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000012"),
                Name = "Iron & ironing board",
                Category = "Convenience",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000013"),
                Name = "Pet friendly",
                Category = "Policy",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000014"),
                Name = "Wheelchair accessible",
                Category = "Accessibility",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000015"),
                Name = "Sea / water view",
                Category = "Views",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000016"),
                Name = "City view",
                Category = "Views",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000017"),
                Name = "Laundry",
                Category = "Services",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            },
            new Amenity
            {
                Id = new Guid("fa000001-0000-4000-8000-000000000018"),
                Name = "Extra bed available",
                Category = "Comfort",
                BusinessRegistrationId = null,
                CreatedAt = SeedTime,
            });
    }
}
