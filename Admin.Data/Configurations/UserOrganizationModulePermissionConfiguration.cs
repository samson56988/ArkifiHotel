using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class UserOrganizationModulePermissionConfiguration : IEntityTypeConfiguration<UserOrganizationModulePermission>
{
    public void Configure(EntityTypeBuilder<UserOrganizationModulePermission> builder)
    {
        builder.ToTable("UserOrganizationModulePermissions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ModuleCode)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasOne(e => e.UserOrganization)
            .WithMany(u => u.ModulePermissions)
            .HasForeignKey(e => e.UserOrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserOrganizationId, e.ModuleCode })
            .IsUnique();
    }
}
