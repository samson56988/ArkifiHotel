using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class UserOrganizationLocationPermissionConfiguration : IEntityTypeConfiguration<UserOrganizationLocationPermission>
{
    public void Configure(EntityTypeBuilder<UserOrganizationLocationPermission> builder)
    {
        builder.ToTable("UserOrganizationLocationPermissions");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.UserOrganization)
            .WithMany(u => u.LocationPermissions)
            .HasForeignKey(e => e.UserOrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.BusinessLocation)
            .WithMany()
            .HasForeignKey(e => e.BusinessLocationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserOrganizationId, e.BusinessLocationId })
            .IsUnique();
    }
}
