using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class UserOrganizationConfiguration : IEntityTypeConfiguration<UserOrganization>
{
    public void Configure(EntityTypeBuilder<UserOrganization> builder)
    {
        builder.ToTable("UserOrganizations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(e => e.Username)
            .HasMaxLength(64);

        builder.Property(e => e.HashedPassword)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.IsSuperAdmin)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.IsDefaultPassword)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.HasAllModuleAccess)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.HasAllLocationAccess)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany(b => b.OrganizationUsers)
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.DefaultLocation)
            .WithMany()
            .HasForeignKey(e => e.DefaultLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.BusinessRegistrationId, e.Email })
            .IsUnique();

        builder.HasIndex(e => new { e.BusinessRegistrationId, e.Username })
            .IsUnique()
            .HasFilter("\"Username\" IS NOT NULL");

        // Only one super-admin per business.
        builder.HasIndex(e => e.BusinessRegistrationId)
            .IsUnique()
            .HasFilter("\"IsSuperAdmin\" = true")
            .HasDatabaseName("IX_UserOrganizations_BusinessRegistrationId_SuperAdmin");
    }
}
