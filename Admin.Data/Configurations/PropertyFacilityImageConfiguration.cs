using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class PropertyFacilityImageConfiguration : IEntityTypeConfiguration<PropertyFacilityImage>
{
    public void Configure(EntityTypeBuilder<PropertyFacilityImage> builder)
    {
        builder.ToTable("PropertyFacilityImages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RelativePath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.OriginalFileName)
            .HasMaxLength(256);

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.PropertyFacilityId);

        builder.HasOne(e => e.PropertyFacility)
            .WithMany(f => f.Images)
            .HasForeignKey(e => e.PropertyFacilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
