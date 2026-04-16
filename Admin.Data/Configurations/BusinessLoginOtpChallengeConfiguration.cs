using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class BusinessLoginOtpChallengeConfiguration : IEntityTypeConfiguration<BusinessLoginOtpChallenge>
{
    public void Configure(EntityTypeBuilder<BusinessLoginOtpChallenge> builder)
    {
        builder.ToTable("BusinessLoginOtpChallenges");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OtpCodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.IsUsed)
            .HasDefaultValue(false);

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => new { e.BusinessRegistrationId, e.IsUsed });

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
