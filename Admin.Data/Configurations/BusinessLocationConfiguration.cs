using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class BusinessLocationConfiguration : IEntityTypeConfiguration<BusinessLocation>
{
    public void Configure(EntityTypeBuilder<BusinessLocation> builder)
    {
        builder.ToTable("BusinessLocations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => new { e.BusinessRegistrationId, e.Name })
            .IsUnique();

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
