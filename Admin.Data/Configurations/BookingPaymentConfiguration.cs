using Admin.Data.Entities;
using Admin.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class BookingPaymentConfiguration : IEntityTypeConfiguration<BookingPayment>
{
    public void Configure(EntityTypeBuilder<BookingPayment> builder)
    {
        builder.ToTable("BookingPayments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
            .HasPrecision(14, 2)
            .IsRequired();

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Method)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(BookingPaymentMethod.Cash);

        builder.Property(e => e.Gateway)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ExternalReference)
            .HasMaxLength(256);

        builder.Property(e => e.Notes)
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => e.BookingId);
        builder.HasIndex(e => new { e.BusinessRegistrationId, e.CreatedAt });

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Booking)
            .WithMany()
            .HasForeignKey(e => e.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
