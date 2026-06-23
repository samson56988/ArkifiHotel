using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RestaurantMenuSettingsConfiguration : IEntityTypeConfiguration<RestaurantMenuSettings>
{
    public void Configure(EntityTypeBuilder<RestaurantMenuSettings> builder)
    {
        builder.ToTable("RestaurantMenuSettings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NavLabel).IsRequired().HasMaxLength(120);
        builder.Property(e => e.HeroEyebrow).IsRequired().HasMaxLength(120);
        builder.Property(e => e.HeroTitle).IsRequired().HasMaxLength(200);
        builder.Property(e => e.HeroSubtitle).HasMaxLength(1000);
        builder.Property(e => e.MealsSectionTitle).IsRequired().HasMaxLength(120);
        builder.Property(e => e.DrinksSectionTitle).IsRequired().HasMaxLength(120);
        builder.Property(e => e.HeroImageRelativePath).HasMaxLength(500);
        builder.Property(e => e.HeroImageOriginalFileName).HasMaxLength(260);
        builder.Property(e => e.Enabled).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.BusinessRegistrationId, e.LocationId }).IsUnique();

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
