using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RestaurantOrderPaymentConfiguration : IEntityTypeConfiguration<RestaurantOrderPayment>
{
    public void Configure(EntityTypeBuilder<RestaurantOrderPayment> builder)
    {
        builder.ToTable("RestaurantOrderPayments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount).HasPrecision(12, 2);
        builder.Property(e => e.Currency).IsRequired().HasMaxLength(8);
        builder.Property(e => e.ExternalReference).HasMaxLength(120);
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => e.ExternalReference);

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RestaurantOrder)
            .WithMany(o => o.Payments)
            .HasForeignKey(e => e.RestaurantOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
