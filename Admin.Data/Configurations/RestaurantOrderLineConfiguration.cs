using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RestaurantOrderLineConfiguration : IEntityTypeConfiguration<RestaurantOrderLine>
{
    public void Configure(EntityTypeBuilder<RestaurantOrderLine> builder)
    {
        builder.ToTable("RestaurantOrderLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.UnitPrice).HasPrecision(12, 2);
        builder.Property(e => e.LineTotal).HasPrecision(12, 2);
        builder.Property(e => e.Quantity).IsRequired();

        builder.HasIndex(e => e.RestaurantOrderId);

        builder.HasOne(e => e.RestaurantOrder)
            .WithMany(o => o.Lines)
            .HasForeignKey(e => e.RestaurantOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
