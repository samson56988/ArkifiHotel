using Admin.Data.Entities;
using Admin.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GuestName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.GuestEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(e => e.GuestPhone)
            .HasMaxLength(40);

        builder.Property(e => e.CheckInDate)
            .IsRequired();

        builder.Property(e => e.CheckOutDate)
            .IsRequired();

        builder.Property(e => e.TotalAmount)
            .HasPrecision(14, 2)
            .IsRequired();

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ConfirmationCode)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(e => e.InternalNotes)
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => e.RoomId);
        builder.HasIndex(e => e.ConfirmationCode)
            .IsUnique();

        builder.HasIndex(e => new { e.BusinessRegistrationId, e.CheckInDate });

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Room)
            .WithMany()
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
