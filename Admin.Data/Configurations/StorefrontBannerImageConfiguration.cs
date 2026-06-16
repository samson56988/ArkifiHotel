using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class StorefrontBannerImageConfiguration : IEntityTypeConfiguration<StorefrontBannerImage>
{
    public void Configure(EntityTypeBuilder<StorefrontBannerImage> builder)
    {
        builder.ToTable("StorefrontBannerImages");

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

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => e.LocationId);

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
