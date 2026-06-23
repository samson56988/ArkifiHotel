using Admin.Data.Entities;
using Admin.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class BusinessRegistrationConfiguration : IEntityTypeConfiguration<BusinessRegistration>
{
    public void Configure(EntityTypeBuilder<BusinessRegistration> builder)
    {
        builder.ToTable("BusinessRegistrations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.BusinessName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ContactEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(e => e.HashedPassword)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(BusinessRegistrationStatus.Inactive);

        builder.Property(e => e.AdminNotes)
            .HasMaxLength(4000);

        builder.Property(e => e.Slug)
            .HasMaxLength(128);

        builder.Property(e => e.LogoPath)
            .HasMaxLength(512);

        builder.Property(e => e.StorefrontThemeJson)
            .HasColumnType("jsonb");

        builder.Property(e => e.BusinessType)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(BusinessType.Hotel);

        builder.Property(e => e.SubscriptionPlanId)
            .IsRequired();

        builder.HasOne(e => e.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(e => e.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.SubscriptionPlanId);
        builder.HasIndex(e => e.SubscriptionExpiresAt);

        builder.HasIndex(e => e.ContactEmail);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasFilter("\"Slug\" IS NOT NULL");
    }
}
