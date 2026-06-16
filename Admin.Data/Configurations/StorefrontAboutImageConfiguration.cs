using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class StorefrontAboutImageConfiguration : IEntityTypeConfiguration<StorefrontAboutImage>
{
    public void Configure(EntityTypeBuilder<StorefrontAboutImage> builder)
    {
        builder.ToTable("StorefrontAboutImages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RelativePath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.OriginalFileName)
            .HasMaxLength(256);

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId)
            .IsUnique();

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
