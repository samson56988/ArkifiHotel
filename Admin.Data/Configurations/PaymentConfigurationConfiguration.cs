using Admin.Data.Entities;
using Admin.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class PaymentConfigurationConfiguration : IEntityTypeConfiguration<PaymentConfiguration>
{
    public void Configure(EntityTypeBuilder<PaymentConfiguration> builder)
    {
        builder.ToTable("PaymentConfigurations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Gateway)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.EncryptedJson)
            .IsRequired()
            .HasMaxLength(8192);

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
