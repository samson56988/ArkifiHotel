using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class EmailVerificationOtpConfiguration : IEntityTypeConfiguration<EmailVerificationOtp>
{
    public void Configure(EntityTypeBuilder<EmailVerificationOtp> builder)
    {
        builder.ToTable("EmailVerificationOtps");

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
