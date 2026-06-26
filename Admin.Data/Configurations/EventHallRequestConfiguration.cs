using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class EventHallRequestConfiguration : IEntityTypeConfiguration<EventHallRequest>
{
    public void Configure(EntityTypeBuilder<EventHallRequest> builder)
    {
        builder.ToTable("EventHallRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GuestName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.GuestEmail).IsRequired().HasMaxLength(320);
        builder.Property(e => e.GuestPhone).IsRequired().HasMaxLength(40);
        builder.Property(e => e.EventPurpose).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.EventHallId);
        builder.HasIndex(e => e.CreatedAt);

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.EventHall)
            .WithMany()
            .HasForeignKey(e => e.EventHallId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
