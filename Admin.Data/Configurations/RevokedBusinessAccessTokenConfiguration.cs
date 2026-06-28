using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RevokedBusinessAccessTokenConfiguration : IEntityTypeConfiguration<RevokedBusinessAccessToken>
{
    public void Configure(EntityTypeBuilder<RevokedBusinessAccessToken> builder)
    {
        builder.ToTable("RevokedBusinessAccessTokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Jti)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(e => e.Jti)
            .IsUnique();

        builder.HasIndex(e => e.ExpiresAtUtc);

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
