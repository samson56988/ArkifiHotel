using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class OrganizationAuditLogConfiguration : IEntityTypeConfiguration<OrganizationAuditLog>
{
    public void Configure(EntityTypeBuilder<OrganizationAuditLog> builder)
    {
        builder.ToTable("OrganizationAuditLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserDisplayName).HasMaxLength(200);
        builder.Property(e => e.UserEmail).HasMaxLength(320);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(64);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(128);
        builder.Property(e => e.LocationName).HasMaxLength(200);
        builder.Property(e => e.Summary).HasMaxLength(500);
        builder.Property(e => e.DetailsJson).HasColumnType("jsonb");

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.BusinessRegistrationId, e.CreatedAt });
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.UserOrganizationId);
    }
}
