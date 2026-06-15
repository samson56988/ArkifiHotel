using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class BusinessSocialProfileConfiguration : IEntityTypeConfiguration<BusinessSocialProfile>
{
    public void Configure(EntityTypeBuilder<BusinessSocialProfile> builder)
    {
        builder.ToTable("BusinessSocialProfiles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FacebookUrl)
            .HasMaxLength(512);

        builder.Property(e => e.InstagramUrl)
            .HasMaxLength(512);

        builder.Property(e => e.TikTokUrl)
            .HasMaxLength(512);

        builder.Property(e => e.XUrl)
            .HasMaxLength(512);

        builder.Property(e => e.ContactEmail)
            .HasMaxLength(320);

        builder.Property(e => e.ContactPhone)
            .HasMaxLength(32);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId)
            .IsUnique();

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
