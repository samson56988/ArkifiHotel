using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RestaurantOrderConfiguration : IEntityTypeConfiguration<RestaurantOrder>
{
    public void Configure(EntityTypeBuilder<RestaurantOrder> builder)
    {
        builder.ToTable("RestaurantOrders");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GuestPhone).IsRequired().HasMaxLength(40);
        builder.Property(e => e.RoomNumber).HasMaxLength(40);
        builder.Property(e => e.OrderNumber).IsRequired().HasMaxLength(32);
        builder.Property(e => e.TotalAmount).HasPrecision(12, 2);
        builder.Property(e => e.Currency).IsRequired().HasMaxLength(8);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.GuestType).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.BusinessRegistrationId);
        builder.HasIndex(e => new { e.BusinessRegistrationId, e.OrderNumber }).IsUnique();
        builder.HasIndex(e => e.CreatedAt);

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
